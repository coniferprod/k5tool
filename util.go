package main

func NewByteSliceFromTwoNybbleRepresentation(data []byte) []byte {
	var result []byte
	count := len(data) / 2 // N.B. Count must be even!
	index := 0
	offset := 0
	for index < count {
		result = append(result, ByteFromTwoNybbles(data[offset], data[offset+1]))
		offset += 2
		index++
	}
	return result
}

func ByteFromTwoNybbles(n1 byte, n2 byte) byte {
	return (n1 << 4) | n2
}
