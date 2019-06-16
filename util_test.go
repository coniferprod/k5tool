package main

import (
	"bytes"
	"testing"
)

func TestByteFromTwoNybbles(t *testing.T) {
	if ByteFromTwoNybbles(0x04, 0x01) != 0x41 {
		t.Error(`ByteFromTwoNybbles(0x04, 0x01) != 0x41`)
	}
}

func TestNewByteSliceFromTwoNybbleRepresentation(t *testing.T) {
	data := []byte{0x04, 0x01, 0x04, 0x0e, 0x04, 0x0e, 0x04, 0x05, 0x05, 0x08, 0x02, 0x00, 0x02, 0x00, 0x03, 0x0f}
	expectedNewData := []byte{0x41, 0x4e, 0x4e, 0x45, 0x58, 0x20, 0x20, 0x3f}
	actualNewData := NewByteSliceFromTwoNybbleRepresentation(data)
	result := bytes.Compare(expectedNewData, actualNewData)
	if result != 0 {
		t.Errorf(`NewByteSliceFromTwoNybbleRepresentation(%q, %q) failed`, expectedNewData, actualNewData)
	}
}
