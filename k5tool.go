package main

import (
	"bytes"
	"fmt"
	"io/ioutil"
	"log"
	"os"
)

type SystemExclusiveHeader struct {
	ManufacturerID byte
	Channel        byte
	Function       byte
	Group          byte
	MachineID      byte
	Substatus1     byte
	Substatus2     byte
}

func main() {
	fmt.Println("Hello, Kawai K5 Digital Multi-Dimensional Synthesizer")

	if len(os.Args) < 2 {
		fmt.Println("Please specify name of Kawai K5 SysEx file")
		os.Exit(-1)
	}

	filename := os.Args[1]
	fmt.Printf("Processing file '%s'\n", filename)

	contents, err := ioutil.ReadFile(filename)
	if err != nil {
		log.Fatal(err)
	} else {
		fmt.Printf("Read %d bytes of data from file\n", len(contents))
	}

	// Many banks have multiple SysEx messages in the same file. Try to split them apart:
	messages := bytes.SplitAfter(contents, []byte{0xF7})
	fmt.Printf("File has %d SysEx messages\n", len(messages))
	for _, msg := range messages {
		if !checkSysEx(msg) {
			continue
		}

		header := getHeader(msg)
		// Process only one block data dumps for now
		if header.Function != 0x20 {
			fmt.Println("Not a one block data dump (function = 20H), ignoring")
			continue
		}

		if header.Substatus1 != 0x00 {
			fmt.Println("Not a SINGLE dump, ignoring")
			continue
		}

		originalData := msg[8 : len(msg)-1] // extract data after SysEx header and before end byte
		//fmt.Println(hex.Dump(originalData))

		data := NewByteSliceFromTwoNybbleRepresentation(originalData)
		//fmt.Println(hex.Dump(data))

		single := NewSingle(data)
		fmt.Printf("Name = '%s'\n", single.Name)
		fmt.Printf("Volume = %d\n", single.Volume)
		fmt.Printf("Balance = %d\n", single.Balance)

		fmt.Println()
	}
}

func getHeader(data []byte) SystemExclusiveHeader {
	return SystemExclusiveHeader{
		ManufacturerID: data[1],
		Channel:        data[2],
		Function:       data[3],
		Group:          data[4],
		MachineID:      data[5],
		Substatus1:     data[6],
		Substatus2:     data[7],
	}
}

// Perform some checks on the SysEx file data.
func checkSysEx(data []byte) bool {
	// The SysEx header is eight bytes long:
	if len(data) < 9 { // we barely have a header
		fmt.Println("Not a valid SysEx file! Too short.")
		return false
	}

	firstByte := data[0]
	lastByte := data[len(data)-1]
	if firstByte != 0xf0 || lastByte != 0xf7 {
		fmt.Println("Not a valid SysEx file! Wrong start and end bytes.")
		return false
	}

	header := SystemExclusiveHeader{
		ManufacturerID: data[1],
		Channel:        data[2],
		Function:       data[3],
		Group:          data[4],
		MachineID:      data[5],
		Substatus1:     data[6],
		Substatus2:     data[7],
	}

	if header.ManufacturerID != 0x40 {
		fmt.Printf("Not a Kawai SysEx file; manufacturer ID should be 40H (found %02xH)\n", header.ManufacturerID)
		return false
	}
	fmt.Printf("%02xH Manufacturer ID = Kawai\n", header.ManufacturerID)

	fmt.Printf("%02xH Channel = %d\n", header.Channel, header.Channel+1)

	functionNames := map[byte]string{
		0x10: "Parameter send",
		0x20: "One block data dump",
		0x21: "All block data dump",
		0x30: "Program send",
		0x40: "Write complete",
		0x41: "Write error",
		0x42: "Write error (protect)",
		0x43: "Write error (no card)",
		0x61: "Machine ID acknowledge",
	}

	fmt.Printf("%02xH Function = %s\n", header.Function, functionNames[header.Function])

	fmt.Printf("%02xH Group = %d\n", header.Group, header.Group)

	if header.MachineID != 0x02 {
		fmt.Printf("Not a Kawai K5 SysEx file; machine ID should be 02H (found %02xH)\n", header.MachineID)
		return false
	}
	fmt.Printf("%02xH Machine ID = K5/K5m\n", header.MachineID)

	substatus1Names := map[byte]string{
		0x00: "SINGLE",
		0x01: "MULTI",
	}
	fmt.Printf("%02xH Sub status 1 = %s\n", header.Substatus1, substatus1Names[header.Substatus1])

	fmt.Printf("%02xH Sub status 2 = %s\n", header.Substatus2, programName(header.Substatus2))

	// The actual SysEx data is 984 bytes per patch
	// (492 significant data bytes expressed as nybbles)
	// A bank of singles has 12 patches. Each patch has 492 data bytes expressed as nybbles,
	// 984 bytes each, for a total of 11,808 bytes.
	// A bank of multis has 12 patches. Each patch has 176 data bytes expressed as nybbles,
	// 352 bytes each, for a total of 4,224 bytes.

	return true
}

func programName(p byte) string {
	bankIndex := p / 12
	bankLetter := "ABCD"[bankIndex]
	programIndex := (p % 12) + 1

	return fmt.Sprintf("%c-%d", bankLetter, programIndex)
}
