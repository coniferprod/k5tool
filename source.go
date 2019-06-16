package main

type Harmonic struct {
	Level              uint8
	IsModulationActive bool  // true if modulation is on for the containing source
	EnvelopeNumber     uint8 // user harmonic envelope number 0/1, 1/2, 2/3 or 3/4

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

type Source struct {
	// DFG
	Coarse int8 // 0~±48
	Fine   int8 // 0~±31

	// DHG
	Harmonics         [64]Harmonic
	HarmonicEnvelopes [6]Envelope
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
