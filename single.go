package main

type Single struct {
	Name    string // max 8 characters (S1~S8)
	Volume  uint8  // 0~63
	Balance int8   // 0~±31
}

func NewSingle(name string) *Single {
	s := new(Single)
	s.Name = name
	s.Volume = 50
	s.Balance = 16
	return s
}
