package main

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
	Segments  [6]EnvelopeSegment // six segments (except DDA env has seven)
	IsLooping bool
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
	Envelope            Envelope
}

type Source struct {
	// DFG
	Coarse int8 // 0~±48
	Fine   int8 // 0~±31

	EnvelopeDepth         int8  // 0~±24
	PressureDepth         int8  // 0~±31
	BenderDepth           int8  // 0~±24  (or 0~24?)
	VelocityEnvelopeDepth int8  // 0~±31
	LFODepth              uint8 // 0~31
	PressureLFODepth      int8  // 0~±31

	Envelope Envelope

	// DHG
	Harmonics         [64]Harmonic
	HarmonicSettings  HarmonicSettings
	HarmonicEnvelopes [6]Envelope

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
