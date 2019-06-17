package main

// Adapted from https://yourbasic.org/golang/bitmask-flag-set-clear/
// Licensed under a CC BY 3.0 license.

const (
	F0 byte = 1 << iota
	F1
	F2
	F3
	F4
	F5
	F6
	F7
)

func SetBits(b, flag byte) byte    { return b | flag }
func ClearBits(b, flag byte) byte  { return b &^ flag }
func ToggleBits(b, flag byte) byte { return b ^ flag }
func HasBit(b, flag byte) bool     { return b&flag != 0 }
