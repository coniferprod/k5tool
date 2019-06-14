import Foundation

public typealias Byte = UInt8

extension Byte {
    public static func fromTwoNybbles(n1: Byte, n2: Byte) -> Byte {
        return (n1 << 4) | n2
    }
    
    // Turns "4C" into "04 0C"
    public func toTwoNybbles() -> (n1: Byte, n2: Byte) {
        return (n1: self >> 4, n2: (self & 0b00001111))
    }
    
    public static func fromTwoNybbleData(data: Data, at offset: Int) -> Byte {
        let n1: Byte = data[offset]
        let n2: Byte = data[offset + 1]
        return Byte.fromTwoNybbles(n1: n1, n2: n2)
    }
}

extension Character {
    public var isAscii: Bool {
        return unicodeScalars.first?.isASCII == true
    }
    public var ascii: UInt32? {
        return isAscii ? unicodeScalars.first?.value : nil
    }
}

extension StringProtocol {
    public var ascii: [UInt32] {
        return compactMap { $0.ascii }
    }
}

extension Data {
    public func differenceAt(_ other: Data) -> Int {
        if self == other {
            return -1
        }
        
        if self.count != other.count {
            return -1
        }
        
        var offset = 0
        while offset < self.count {
            if self[offset] != other[offset] {
                return offset
            }
            offset += 1
        }
        
        return -1
    }
    
    public func fromTwoNybbleRepresentation() -> Data {
        var result = Data()
        let normalByteCount = self.count / 2  // N.B. Count must be even!
        var index = 0
        var offset = 0
        while index < normalByteCount {
            result += [Byte.fromTwoNybbleData(data: self, at: offset)]
            offset += 2
            index += 1
        }
        return result
    }
    
    public func toTwoNybbleRepresentation() -> Data {
        var result = Data()
        
        for b in self {
            let n = b.toTwoNybbles()
            result.append(n.n1)
            result.append(n.n2)
        }
        
        return result
    }
}

extension Data {
    public func hexEncodedString(uppercase: Bool = true) -> String {
        let format = uppercase ? "%02hhX" : "%02hhx"
        return map { String(format: format, $0) }.joined(separator: " ")
    }
    
    public func decimalString() -> String {
        return map { String(format: "%02d", $0) }.joined(separator: " ")
    }
}

extension String {
    public func leftPadding(toLength: Int, withPad character: Character) -> String {
        let stringLength = self.count
        if stringLength < toLength {
            return String(repeatElement(character, count: toLength - stringLength)) + self
        } else {
            return String(self.suffix(toLength))
        }
    }
}
