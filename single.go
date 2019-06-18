package main

import "fmt"

type PicMode uint8

const (
	Source1 PicMode = 0
	Source2 PicMode = 1
	Both    PicMode = 2
)

type LFOShape uint8

const (
	Triangle         LFOShape = 1
	InverseTriangle  LFOShape = 2
	Square           LFOShape = 3
	InverseSquare    LFOShape = 4
	Sawtooth         LFOShape = 5
	InvertedSawtooth LFOShape = 6
)

type LFO struct {
	Shape LFOShape
	Speed uint8 // 0~99
	Delay uint8 // 0~31
	Trend uint8 // 0~31
}

func (l LFO) GenerateData() []byte {
	d := []byte{}
	d = append(d, byte(l.Shape), l.Speed, l.Delay, l.Trend)
	return d
}

type Formant struct {
	IsActive bool
	Levels   [11]uint8 // 11 bands, corresponding to C-1 ~ C9, value 0~99
}

func (f Formant) GenerateData() []byte {
	d := []byte{}
	for i := 0; i < 11; i++ {
		levelValue := f.Levels[i]
		if i == 0 {
			if f.IsActive {
				levelValue = SetBits(levelValue, F7)
			}
		}
		d = append(d, levelValue)
	}
	return d
}

type Single struct {
	Name    string // max 8 characters (S1~S8)
	Volume  uint8  // 0~63
	Balance int8   // 0~±31

	Source1Settings SourceSettings
	Source2Settings SourceSettings

	Portamento      bool
	PortamentoSpeed uint8 // 0~63
	Mode            SourceMode
	PicMode         PicMode

	// DFG
	Source1 Source
	Source2 Source

	LFO     LFO     // LFO
	Formant Formant // DFT

	Checksum uint16 // the Kawai checksum
}

func getNextByte(d []byte, offset int) (byte, int) {
	//fmt.Printf("%04X\n", offset)
	return d[offset], offset + 1
}

func NewSingle(d []byte) *Single {
	s := new(Single)

	// Brute-forcing the name in: S1 ... S8
	s.Name = string([]byte{d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7]})

	offset := 8
	b, offset := getNextByte(d, offset) // S9
	s.Volume = b
	b, offset = getNextByte(d, offset) // S10
	s.Balance = int8(b)

	// Source 1 and source 2 settings.
	// Note that the pedal assign and the wheel assign for one source are in the same byte
	s.Source1Settings = SourceSettings{
		Delay:       d[offset],                     // S11
		PedalDepth:  d[offset+2],                   // S13
		WheelDepth:  d[offset+4],                   // S15
		PedalAssign: ModulationAssign(d[offset+6]), // S17 high nybble
		WheelAssign: ModulationAssign(d[offset+6]), // S17 low nybble
	}

	s.Source2Settings = SourceSettings{
		Delay:       d[offset+1],                   // S12
		PedalDepth:  d[offset+3],                   // S14
		WheelDepth:  d[offset+5],                   // S16
		PedalAssign: ModulationAssign(d[offset+7]), // S18 high nybble
		WheelAssign: ModulationAssign(d[offset+7]), // S18 low nybble
	}

	offset += 8

	// portamento and p. speed - S19
	b, offset = getNextByte(d, offset)
	s.Portamento = HasBit(b, F7)
	s.PortamentoSpeed = b & 0x3f // 0b00111111

	// mode and "pic mode" - S20
	b, offset = getNextByte(d, offset)
	if HasBit(b, F2) {
		s.Mode = Full
	} else {
		s.Mode = Twin
	}
	picModeValue := b & 0x03
	switch picModeValue {
	case 0:
		s.PicMode = Source1
	case 1:
		s.PicMode = Source2
	case 2:
		s.PicMode = Both
	default:
		s.PicMode = Both
	}

	endOffset := 478
	sd := d[offset:endOffset]
	fmt.Printf("source data = %d bytes, from %d (%04XH) to %d (%04XH)\n", len(sd), offset, offset, endOffset, endOffset)

	s1d := []byte{}
	s2d := []byte{}
	for i := 0; i < len(sd); i++ {
		if i%2 == 0 {
			s1d = append(s1d, sd[i])
		} else {
			s2d = append(s2d, sd[i])
		}
	}
	fmt.Printf("s1 data = %d bytes, s2 data = %d bytes\n", len(s1d), len(s2d))

	source1Data := []byte{}
	source1Data = append(source1Data, d[0:20]...)
	source1Data = append(source1Data, s1d...)
	s.Source1 = *NewSource(s1d)

	source2Data := []byte{}
	source2Data = append(source2Data, d[0:20]...)
	source2Data = append(source2Data, s1d...)
	s.Source2 = *NewSource(s2d)

	offset += len(sd)

	s.LFO = LFO{
		Shape: LFOShape(d[offset]),
		Speed: d[offset+1],
		Delay: d[offset+2],
		Trend: d[offset+3],
	}

	formant := Formant{
		IsActive: false,
		Levels:   [11]uint8{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
	}

	for i := 0; i < 11; i++ {
		b, offset = getNextByte(d, offset)
		if i == 0 {
			HasBit(b, F7)
		}
		formant.Levels[i] = b & 0x7f
	}
	s.Formant = formant

	b, offset = getNextByte(d, offset)
	checksumLow := b

	b, offset = getNextByte(d, offset)
	checksumHigh := b

	s.Checksum = (uint16(checksumHigh) << 8) | uint16(checksumLow)

	return s
}

func (s Single) GenerateData() []byte {
	d := []byte{}

	d = append(d, s.Name...)
	d = append(d, s.Volume, byte(s.Balance))
	d = append(d, s.Source1Settings.Delay, s.Source2Settings.Delay)
	d = append(d, s.Source1Settings.PedalDepth, s.Source2Settings.PedalDepth)
	d = append(d, s.Source1Settings.WheelDepth, s.Source2Settings.WheelDepth)
	d = append(d, (byte(s.Source1Settings.PedalAssign)<<4)|byte(s.Source1Settings.WheelAssign))
	d = append(d, (byte(s.Source2Settings.PedalAssign)<<4)|byte(s.Source2Settings.WheelAssign))

	portamentoValue := s.PortamentoSpeed
	if s.Portamento {
		SetBits(portamentoValue, F7)
	}
	d = append(d, portamentoValue)

	modeValue := byte(s.PicMode) | byte(s.Mode<<2)
	d = append(d, modeValue)

	source1Data := s.Source1.GenerateData()
	source2Data := s.Source2.GenerateData()
	// Interleave S1 and S2 data, then append result
	sd := []byte{}
	for i := 0; i < len(source1Data); i++ {
		sd = append(sd, source1Data[i], source2Data[i])
	}
	d = append(d, sd...)

	lfoData := s.LFO.GenerateData()
	d = append(d, lfoData...)

	d = append(d, byte(s.Source1.KeyScaling.Right), byte(s.Source2.KeyScaling.Right))
	d = append(d, byte(s.Source1.KeyScaling.Left), byte(s.Source2.KeyScaling.Left))
	d = append(d, s.Source1.KeyScaling.Breakpoint, s.Source2.KeyScaling.Breakpoint)

	formantData := s.Formant.GenerateData()
	d = append(d, formantData...)

	d = append(d, byte(0)) // S490
	sum := getChecksum(d)
	d = append(d, byte(sum&0xff), byte(((sum >> 8) & 0xff)))

	return d
}

func getChecksum(d []byte) uint16 {
	var sum uint16
	for i := 0; i < len(d); i += 2 {
		sum += (((uint16(d[i+1]) & 0xff) << 8) | uint16((d[i] & 0xff)))
	}
	sum &= 0xffff
	sum = (0x53ac - sum) & 0xffff
	return sum
}
