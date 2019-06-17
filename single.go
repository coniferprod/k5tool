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

type Formant struct {
	IsActive bool
	Levels   [11]uint8 // 11 bands, corresponding to C-1 ~ C9, value 0~99
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

	Checksum uint16  // the Kawai checksum
}

func getNextByte(d []byte, offset int) (byte, int) {
	return d[offset], offset + 1
}

func NewSingle(d []byte) *Single {
	s := new(Single)

	// Brute-forcing the name in...
	s.Name = string([]byte{d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7]})

	offset := 8
	b, offset := getNextByte(d, offset)
	s.Volume = b
	b, offset = getNextByte(d, offset)
	s.Balance = int8(b)

	s.Source1Settings = SourceSettings{
		Delay:       d[offset],
		PedalDepth:  d[offset+2],
		WheelDepth:  d[offset+4],
		PedalAssign: ModulationAssign(d[offset+6]),
		WheelAssign: ModulationAssign(d[offset+8]),
	}

	s.Source2Settings = SourceSettings{
		Delay:       d[offset+1],
		PedalDepth:  d[offset+3],
		WheelDepth:  d[offset+5],
		PedalAssign: ModulationAssign(d[offset+7]),
		WheelAssign: ModulationAssign(d[offset+9]),
	}

	offset += 10

	// portamento and p. speed - S19
	b, offset = getNextByte(d, offset)
	s.Portamento = HasBit(b, F7)
	s.PortamentoSpeed = b & 0x3f // 0b00111111

	// mode - S20
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

	sd := d[offset : offset+468]
	fmt.Printf("source data = %d bytes\n", len(sd))

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

	s.Source1 = *NewSource(s1d)
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
		Levels: []uint8{0,0,0,0,0,0,0,0,0,0,0}
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
