package main

import (
	"testing"
)

func TestHasBitTrue(t *testing.T) {
	b1 := byte(0xff)
	if !HasBit(b1, F7) {
		t.Error(`HasBit(0xff, F7) != true`)
	}
}

func TestHasBitFalse(t *testing.T) {
	b2 := byte(0x7f)
	if HasBit(b2, F7) {
		t.Error(`HasBit(0x7f, F7) != false`)
	}
}
