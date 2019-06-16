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
