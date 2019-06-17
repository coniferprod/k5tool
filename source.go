package main

import "fmt"

type Harmonic struct {
	Level              uint8
	IsModulationActive bool  // true if modulation is on for the containing source
	EnvelopeNumber     uint8 // user harmonic envelope number 0/1, 1/2, 2/3 or 3/4

}

type HarmonicSelection uint8

const (
	Live HarmonicSelection = 0
	Die  HarmonicSelection = 1
	All  HarmonicSelection = 2
)

type HarmonicModulation struct {
	IsOn           bool
	EnvelopeNumber uint8
}

type HarmonicSettings struct {
	VelocityDepth   int8  // 0~±31
	PressureDepth   int8  // 0~±31
	KeyScalingDepth int8  // 0~±31
	LFODepth        uint8 // 0~31

	EnvelopeSettings   [4]HarmonicEnvelopeSettings
	IsModulationActive bool
	Selection          HarmonicSelection

	RangeFrom uint8 // 1~63
	RangeTo   uint8 // 1~63

	Odd    HarmonicModulation
	Even   HarmonicModulation
	Octave HarmonicModulation
	Fifth  HarmonicModulation
	All    HarmonicModulation

	Angle  uint8 // 0/-, 1/0, 1/+ (maybe should be 2/+ ?)
	Number uint8 // 1~63

	IsShadowOn bool
}

type HarmonicEnvelopeSettings struct {
	IsActive bool
	Effect   uint8 // 0~31 (SysEx manual says "s<x> env<y> off", maybe should be "eff"?)
}

type EnvelopeSegment struct {
	Rate  byte
	Level byte
	IsMax bool
	IsMod bool
}

type Envelope struct {
	Segments [6]EnvelopeSegment // six segments (except DDA env has seven)
}

type AmplifierEnvelope struct {
	Segments [7]EnvelopeSegment
}

type Filter struct {
	IsActive              bool  // param 105
	IsModulationActive    bool  // param 106
	Cutoff                uint8 // 0~99
	CutoffModulation      uint8 // 0~31
	Slope                 uint8 // 0~31
	SlopeModulation       uint8 // 0~31
	FlatLevel             uint8 // 0~31
	VelocityDepth         int8  // 0~±31
	PressureDepth         int8  // 0~±31
	KeyScalingDepth       int8  // 0~±31
	EnvelopeDepth         int8  // 0~±31
	VelocityEnvelopeDepth int8  // 0~±31
	LFODepth              uint8 // 0~31
}

type KeyScaling struct {
	Right      int8  // 0~±31
	Left       int8  // 0~±31
	Breakpoint uint8 // 0~127
}

type Amplifier struct {
	IsActive            bool
	AttackVelocityDepth int8  // 0~±31
	PressureDepth       int8  // 0~±31
	KeyScalingDepth     int8  // 0~±31
	LFODepth            uint8 // 0~31
	AttackVelocityRate  int8  // 0~±15
	ReleaseVelocityRate int8  // 0~±15
	KeyScalingRate      int8  // 0~±15
	Envelope            AmplifierEnvelope
}

type Source struct {
	// DFG
	Coarse int8 // 0~±48
	Fine   int8 // 0~±31

	// Since Go has no discriminated union, use enum and separate key value
	KeyTracking KeyTracking
	Key         uint8

	EnvelopeDepth         int8  // 0~±24
	PressureDepth         int8  // 0~±31
	BenderDepth           int8  // 0~±24  (or 0~24?)
	VelocityEnvelopeDepth int8  // 0~±31
	LFODepth              uint8 // 0~31
	PressureLFODepth      int8  // 0~±31

	EnvelopeLooping bool
	Envelope        Envelope

	// DHG
	Harmonics         [63]Harmonic
	HarmonicSettings  HarmonicSettings
	HarmonicEnvelopes [4]Envelope

	// DDF (S381~S426)
	Filter         Filter
	FilterEnvelope Envelope

	// KS
	KeyScaling KeyScaling

	// DDA
	Amplifier Amplifier
}

type SourceSettings struct {
	Delay       uint8 // 0~31
	PedalDepth  uint8 // 0~31
	WheelDepth  uint8 // 0~31
	PedalAssign ModulationAssign
	WheelAssign ModulationAssign
}

type ModulationAssign uint8

const (
	DFGLFO ModulationAssign = 0
	DHG    ModulationAssign = 1
	Cutoff ModulationAssign = 2
	Slope  ModulationAssign = 3
	Off    ModulationAssign = 4
)

type SourceMode uint8

const (
	Twin SourceMode = 0
	Full SourceMode = 1
)

type KeyTracking uint8

const (
	Track KeyTracking = 0
	Fixed KeyTracking = 1 // need also the fixed key
)

func NewSource(d []byte) *Source {
	fmt.Printf("NewSource: len(d) = %d\n", len(d))
	s := new(Source)

	offset := 0
	b, offset := getNextByte(d, offset)
	s.Coarse = int8(b)

	b, offset = getNextByte(d, offset)
	s.Fine = int8(b)

	b, offset = getNextByte(d, offset)
	if HasBit(b, F7) {
		s.KeyTracking = Fixed
		s.Key = b & 0x7f
	} else {
		s.KeyTracking = Track
		// Key is ignored in this case
	}

	b, offset = getNextByte(d, offset)
	s.EnvelopeDepth = int8(b)

	b, offset = getNextByte(d, offset)
	s.PressureDepth = int8(b)

	b, offset = getNextByte(d, offset)
	s.BenderDepth = int8(b)

	b, offset = getNextByte(d, offset)
	s.VelocityEnvelopeDepth = int8(b)

	b, offset = getNextByte(d, offset)
	s.LFODepth = b

	b, offset = getNextByte(d, offset)
	s.PressureLFODepth = int8(b)

	segments := [6]EnvelopeSegment{
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
		{Rate: 0, Level: 0, IsMax: false, IsMod: false},
	}

	// Envelope looping on/off is bit7 of S39
	// N.B. Don't advance offset since we need to start at S39 with the segments
	b = d[offset]
	s.EnvelopeLooping = HasBit(b, F7)
	for i := 0; i < 6; i++ {
		b, offset = getNextByte(d, offset)
		rate := b
		if i == 0 { // remove the "envelope looping" bit from the first value
			rate = b & 0x7f
		}
		segments[i].Rate = rate
	}

	// Another round for the levels
	for i := 0; i < 6; i++ {
		b, offset = getNextByte(d, offset)
		segments[i].Level = b
	}

	s.Envelope = Envelope{
		Segments: segments,
	}

	// DHG
	for i := 0; i < 63; i++ {
		b, offset = getNextByte(d, offset)
		s.Harmonics[i] = Harmonic{
			Level: b,
		}
	}

	count := 0
	for count < 62 {
		if count%2 == 0 {
			b, offset = getNextByte(d, offset)
		}

		odd := b & 0x0f // 0b00001111
		even := (b & 0xf0) >> 4

		s.Harmonics[count].IsModulationActive = HasBit(odd, F3)
		s.Harmonics[count].EnvelopeNumber = odd & 0x03

		s.Harmonics[count+1].IsModulationActive = HasBit(even, F3)
		s.Harmonics[count+1].EnvelopeNumber = even & 0x03

		count += 2
	}

	// It appears that the harm63 settings are duplicated in the SysEx manual:
	// S251 and S252 have the same description. Need to investigate this more;
	// in the meantime, just use the first one.
	b, offset = getNextByte(d, offset)
	odd := b & 0x0f // 0b00001111
	//even := (b & 0xf0) >> 4
	s.Harmonics[62].IsModulationActive = HasBit(odd, F3)
	s.Harmonics[62].EnvelopeNumber = odd & 0x03

	// DHG harmonic settings
	harmSet := HarmonicSettings{
		EnvelopeSettings: [4]HarmonicEnvelopeSettings{},
	}
	b, offset = getNextByte(d, offset)
	harmSet.VelocityDepth = int8(b)
	b, offset = getNextByte(d, offset)
	harmSet.PressureDepth = int8(b)
	b, offset = getNextByte(d, offset)
	harmSet.KeyScalingDepth = int8(b)
	b, offset = getNextByte(d, offset)
	harmSet.LFODepth = b

	for i := 0; i < 4; i++ {
		b, offset = getNextByte(d, offset)
		hes := HarmonicEnvelopeSettings{
			IsActive: HasBit(b, F7),
			Effect:   b & 0x1f,
		}
		harmSet.EnvelopeSettings[i] = hes
	}

	b, offset = getNextByte(d, offset)
	harmSet.IsModulationActive = HasBit(b, F7)

	var selection HarmonicSelection
	switch b & 0x03 {
	case 0:
		selection = Live
	case 1:
		selection = Die
	case 2:
		selection = All
	default:
		selection = All
	}
	harmSet.Selection = selection

	b, offset = getNextByte(d, offset)
	harmSet.RangeFrom = b

	b, offset = getNextByte(d, offset)
	harmSet.RangeTo = b

	//fmt.Printf("before HE selections (S275), offset = %d (%04XH)\n", offset+20, offset+20)

	// Harmonic envelope selections = 0/1, 1/2, 2/3, 3/4
	b, offset = getNextByte(d, offset)
	harmSet.Odd = HarmonicModulation{
		IsOn:           HasBit(b, F7),
		EnvelopeNumber: ((b & 0x30) >> 4) + 1,
	}
	harmSet.Even = HarmonicModulation{
		IsOn:           HasBit(b, F3),
		EnvelopeNumber: (b & 0x03) + 1,
	}

	b, offset = getNextByte(d, offset)
	harmSet.Octave = HarmonicModulation{
		IsOn:           HasBit(b, F7),
		EnvelopeNumber: ((b & 0x30) >> 4) + 1,
	}
	harmSet.Fifth = HarmonicModulation{
		IsOn:           HasBit(b, F3),
		EnvelopeNumber: (b & 0x03) + 1,
	}

	b, offset = getNextByte(d, offset)
	harmSet.All = HarmonicModulation{
		IsOn:           HasBit(b, F7),
		EnvelopeNumber: ((b & 0x30) >> 4) + 1,
	}

	b, offset = getNextByte(d, offset)
	harmSet.Angle = b

	b, offset = getNextByte(d, offset)
	harmSet.Number = b

	b, offset = getNextByte(d, offset)
	harmSet.IsShadowOn = HasBit(b, F7)

	s.HarmonicSettings = harmSet

	// We wanted the high bit of the first envelope segment value,
	// so back up the offset by one byte.
	offset--

	// Next, the harmonic envelopes (four of them):
	envelopes := [4]Envelope{}

	envelopeIndex := 0
	for envelopeIndex < 4 {
		segments := [6]EnvelopeSegment{}
		segmentIndex := 0
		for segmentIndex < 6 {
			b, offset = getNextByte(d, offset)
			segments[segmentIndex].Level = b & 0x3f
			segments[segmentIndex].IsMax = HasBit(b, F6)
			segmentIndex++
		}

		segmentIndex = 0
		for segmentIndex < 6 {
			b, offset = getNextByte(d, offset)
			segments[segmentIndex].Rate = b
			segmentIndex++
		}

		envelopes[envelopeIndex].Segments = segments
		envelopeIndex++
	}
	s.HarmonicEnvelopes = envelopes

	filter, offset := readFilter(d, offset)
	s.Filter = filter

	// Filter envelope
	filterEnvelopeSegments := [6]EnvelopeSegment{}
	segmentIndex := 0
	for segmentIndex < 6 {
		b, offset = getNextByte(d, offset)
		filterEnvelopeSegments[segmentIndex].Rate = b
		segmentIndex++
	}
	//fmt.Printf("offset = %04X\n", offset)
	segmentIndex = 0
	for segmentIndex < 6 {
		b, offset = getNextByte(d, offset)
		//fmt.Printf("offset = %04X\n", offset)
		filterEnvelopeSegments[segmentIndex].IsMax = HasBit(b, F6)
		filterEnvelopeSegments[segmentIndex].Level = b & 0x3f
		segmentIndex++
	}
	filterEnvelope := Envelope{
		Segments: filterEnvelopeSegments,
	}
	s.FilterEnvelope = filterEnvelope

	amplifier, offset := readAmplifier(d, offset)
	s.Amplifier = amplifier

	// Amp envelope
	ampEnvSeg := [7]EnvelopeSegment{} // note one extra segment in DDA ENV
	segmentIndex = 0
	for segmentIndex < 7 {
		b, offset = getNextByte(d, offset)
		ampEnvSeg[segmentIndex].IsMod = HasBit(b, F6)
		ampEnvSeg[segmentIndex].Rate = b & 0x3f
		segmentIndex++
	}

	segmentIndex = 0
	for segmentIndex < 7 {
		b, offset = getNextByte(d, offset)
		ampEnvSeg[segmentIndex].IsMax = HasBit(b, F6)
		ampEnvSeg[segmentIndex].Level = b & 0x3f
		segmentIndex++
	}

	s.Amplifier.Envelope = AmplifierEnvelope{
		Segments: ampEnvSeg,
	}

	ks := KeyScaling{}
	fmt.Printf("offset = %04X\n", offset)
	b, offset = getNextByte(d, offset)
	ks.Right = int8(b)
	b, offset = getNextByte(d, offset)
	ks.Left = int8(b)
	b, offset = getNextByte(d, offset)
	ks.Breakpoint = b
	s.KeyScaling = ks

	return s
}

func readFilter(d []byte, at int) (Filter, int) {
	// DDF
	filter := Filter{}
	offset := at
	var b byte
	b, offset = getNextByte(d, offset)
	filter.Cutoff = b
	b, offset = getNextByte(d, offset)
	filter.CutoffModulation = b
	b, offset = getNextByte(d, offset)
	filter.Slope = b
	b, offset = getNextByte(d, offset)
	filter.SlopeModulation = b
	b, offset = getNextByte(d, offset)
	filter.FlatLevel = b
	b, offset = getNextByte(d, offset)
	filter.VelocityDepth = int8(b)
	b, offset = getNextByte(d, offset)
	filter.PressureDepth = int8(b)
	b, offset = getNextByte(d, offset)
	filter.KeyScalingDepth = int8(b)
	b, offset = getNextByte(d, offset)
	filter.EnvelopeDepth = int8(b)
	b, offset = getNextByte(d, offset)
	filter.VelocityEnvelopeDepth = int8(b)
	b, offset = getNextByte(d, offset)
	filter.IsActive = HasBit(b, F7)
	filter.IsModulationActive = HasBit(b, F6)
	filter.LFODepth = b & 0x1f
	return filter, offset
}

func readAmplifier(d []byte, at int) (Amplifier, int) {
	offset := at
	var b byte
	amp := Amplifier{}
	b, offset = getNextByte(d, offset)
	amp.AttackVelocityDepth = int8(b)
	b, offset = getNextByte(d, offset)
	amp.PressureDepth = int8(b)
	b, offset = getNextByte(d, offset)
	amp.KeyScalingDepth = int8(b)
	b, offset = getNextByte(d, offset)
	amp.IsActive = HasBit(b, F7)
	amp.LFODepth = b & 0x7f
	b, offset = getNextByte(d, offset)
	amp.AttackVelocityRate = int8(b)
	b, offset = getNextByte(d, offset)
	amp.ReleaseVelocityRate = int8(b)
	b, offset = getNextByte(d, offset)
	amp.KeyScalingRate = int8(b)
	return amp, offset
}

func (s Source) GenerateData() []byte {
	d := []byte{}

	d = append(d, byte(s.Coarse), byte(s.Fine))

	var keyTrackingValue byte
	if s.KeyTracking == Track {
		keyTrackingValue = 0
	} else if s.KeyTracking == Fixed {
		keyTrackingValue = s.Key
		keyTrackingValue = SetBits(keyTrackingValue, F7)
	}
	d = append(d, keyTrackingValue)

	d = append(d, byte(s.EnvelopeDepth), byte(s.PressureDepth), byte(s.BenderDepth), byte(s.VelocityEnvelopeDepth), s.LFODepth, byte(s.PressureLFODepth))

	// DFG envelope
	for i := 0; i < len(s.Envelope.Segments); i++ {
		rateValue := s.Envelope.Segments[i].Rate
		if i == 0 && s.EnvelopeLooping {
			rateValue = SetBits(rateValue, F7)
		}
		d = append(d, rateValue)
	}

	for i := 0; i < len(s.Envelope.Segments); i++ {
		d = append(d, s.Envelope.Segments[i].Level)
	}

	// DHG
	for i := 0; i < len(s.Harmonics); i++ {
		d = append(d, s.Harmonics[i].Level)
	}

	// First handle harmonics 1...62. They are packed two to a byte.
	harmNum := 0
	byteCount := 0
	for byteCount < 31 {
		h1 := s.Harmonics[harmNum]
		harmNum++
		value := h1.EnvelopeNumber
		if h1.IsModulationActive {
			value = SetBits(value, F3)
		}
		h2 := s.Harmonics[harmNum]
		harmNum++
		value |= (h2.EnvelopeNumber << 4)
		if h2.IsModulationActive {
			value = SetBits(value, F7)
		}
		byteCount++
		d = append(d, value)
	}

	// Handle the last harmonic (63rd)
	harm := s.Harmonics[62]
	harmValue := harm.EnvelopeNumber
	if harm.IsModulationActive {
		harmValue = SetBits(harmValue, F3)
	}
	d = append(d, harmValue)

	// Duplicate the 63rd harmonic for now (see parsing section for explanation)
	d = append(d, harmValue)

	d = append(d, byte(s.HarmonicSettings.VelocityDepth), byte(s.HarmonicSettings.PressureDepth), byte(s.HarmonicSettings.KeyScalingDepth))

	for i := 0; i < len(s.HarmonicSettings.EnvelopeSettings); i++ {
		harmEnv := s.HarmonicSettings.EnvelopeSettings[i]
		effectValue := harmEnv.Effect
		if harmEnv.IsActive {
			effectValue = SetBits(effectValue, F7)
		}
		d = append(d, effectValue)
	}

	// Harmonic modulation
	harmSel := byte(s.HarmonicSettings.Selection)
	if s.HarmonicSettings.IsModulationActive {
		harmSel = SetBits(harmSel, F7)
	}
	d = append(d, harmSel)

	d = append(d, s.HarmonicSettings.RangeFrom, s.HarmonicSettings.RangeTo)

	// Harmonic envelope selections = 0/1, 1/2, 2/3, 3/4
	harmSel = (s.HarmonicSettings.Odd.EnvelopeNumber - 1) << 4
	if s.HarmonicSettings.Odd.IsOn {
		harmSel = SetBits(harmSel, F7)
	}
	harmSel |= s.HarmonicSettings.Even.EnvelopeNumber - 1
	if s.HarmonicSettings.Even.IsOn {
		harmSel = SetBits(harmSel, F3)
	}
	d = append(d, harmSel)

	harmSel = (s.HarmonicSettings.Octave.EnvelopeNumber - 1) << 4
	if s.HarmonicSettings.Octave.IsOn {
		harmSel = SetBits(harmSel, F7)
	}
	harmSel |= s.HarmonicSettings.Fifth.EnvelopeNumber - 1
	if s.HarmonicSettings.Fifth.IsOn {
		harmSel = SetBits(harmSel, F3)
	}
	d = append(d, harmSel)

	harmSel = (s.HarmonicSettings.All.EnvelopeNumber - 1) << 4
	if s.HarmonicSettings.All.IsOn {
		harmSel = SetBits(harmSel, F7)
	}
	d = append(d, harmSel)

	d = append(d, s.HarmonicSettings.Angle, s.HarmonicSettings.Number)

	// Harmonic envelopes
	for ei := 0; ei < len(s.HarmonicEnvelopes); ei++ {
		for si := 0; si < len(s.HarmonicEnvelopes[ei].Segments); si++ {
			levelValue := s.HarmonicEnvelopes[ei].Segments[si].Level
			if s.HarmonicEnvelopes[ei].Segments[si].IsMax {
				levelValue = SetBits(levelValue, F6)
			}

			// Handle special case in first value
			if ei == 0 && si == 0 {
				if s.HarmonicSettings.IsShadowOn {
					levelValue = SetBits(levelValue, F7)
				}
			}

			d = append(d, levelValue)
		}

		for si := 0; si < len(s.HarmonicEnvelopes[ei].Segments); si++ {
			d = append(d, s.HarmonicEnvelopes[ei].Segments[si].Rate)
		}
	}

	// The filter
	d = append(d, s.Filter.Cutoff, s.Filter.CutoffModulation, s.Filter.Slope, s.Filter.SlopeModulation, s.Filter.FlatLevel)
	d = append(d, byte(s.Filter.VelocityDepth), byte(s.Filter.PressureDepth), byte(s.Filter.KeyScalingDepth), byte(s.Filter.EnvelopeDepth), byte(s.Filter.VelocityEnvelopeDepth))

	filterValue := s.Filter.LFODepth
	if s.Filter.IsActive {
		filterValue = SetBits(filterValue, F7)
	}
	if s.Filter.IsModulationActive {
		filterValue = SetBits(filterValue, F6)
	}
	d = append(d, filterValue)

	// Filter envelope
	for i := 0; i < len(s.FilterEnvelope.Segments); i++ {
		d = append(d, s.FilterEnvelope.Segments[i].Rate)
	}

	for i := 0; i < len(s.FilterEnvelope.Segments); i++ {
		levelValue := s.FilterEnvelope.Segments[i].Level
		if s.FilterEnvelope.Segments[i].IsMax {
			levelValue = SetBits(levelValue, F6)
		}
		d = append(d, levelValue)
	}

	d = append(d, byte(s.Amplifier.AttackVelocityDepth), byte(s.Amplifier.PressureDepth), byte(s.Amplifier.KeyScalingDepth))

	depthValue := s.Amplifier.LFODepth
	if s.Amplifier.IsActive {
		depthValue = SetBits(depthValue, F7)
	}
	d = append(d, depthValue)

	d = append(d, byte(s.Amplifier.AttackVelocityRate), byte(s.Amplifier.ReleaseVelocityRate), byte(s.Amplifier.KeyScalingRate))

	// Amp envelope. N.B. This envelope has seven rates but only six levels.
	for i := 0; i < len(s.Amplifier.Envelope.Segments); i++ {
		rateValue := s.Amplifier.Envelope.Segments[i].Rate
		if s.Amplifier.Envelope.Segments[i].IsMod {
			rateValue = SetBits(rateValue, F6)
		}
		d = append(d, rateValue)
	}

	for i := 0; i < len(s.Amplifier.Envelope.Segments); i++ {
		levelValue := s.Amplifier.Envelope.Segments[i].Level
		if s.Amplifier.Envelope.Segments[i].IsMax {
			levelValue = SetBits(levelValue, F6)
		}
		if i == 6 {
			// No level for amp env seg 7
			d = append(d, byte(0))
		} else {
			d = append(d, levelValue)
		}
	}

	// NOTE: Key scaling data is generated here, but in the Single.

	return d
}
