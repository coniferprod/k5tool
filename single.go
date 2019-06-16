package main

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
}

func NewSingle(name string) *Single {
	s := new(Single)
	s.Name = name
	s.Volume = 50
	s.Balance = 16
	return s
}
