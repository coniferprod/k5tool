import Foundation

import BinarySwift


typealias Byte = UInt8

extension UInt8 {
    static func fromTwoNybbles(n1: UInt8, n2: UInt8) -> UInt8 {
        return (n1 << 4) | n2
    }
    
    // Turns "4C" into "04 0C"
    func toTwoNybbles() -> (n1: UInt8, n2: UInt8) {
        return (n1: self >> 4, n2: (self & 0b00001111))
    }
    
    static func twoNybbleValue(data: BinaryData, at offset: Int) throws -> UInt8 {
        let n1: UInt8 = try data.get(offset)
        let n2: UInt8 = try data.get(offset + 1)
        return UInt8.fromTwoNybbles(n1: n1, n2: n2)
    }
    
    static func fromTwoNybbleData(data: Data, at offset: Int) -> UInt8 {
        let n1: UInt8 = data[offset]
        let n2: UInt8 = data[offset + 1]
        return UInt8.fromTwoNybbles(n1: n1, n2: n2)
    }
}

extension BinaryData {
    func getTwoNybbleValue(at offset: Int) throws -> (UInt8, Int) {
        let n1: UInt8 = try self.get(offset)
        let n2: UInt8 = try self.get(offset + 1)
        return (UInt8.fromTwoNybbles(n1: n1, n2: n2), offset + 2)
    }
}

extension Character {
    var isAscii: Bool {
        return unicodeScalars.first?.isASCII == true
    }
    var ascii: UInt32? {
        return isAscii ? unicodeScalars.first?.value : nil
    }
}

extension StringProtocol {
    var ascii: [UInt32] {
        return compactMap { $0.ascii }
    }
}

extension Data {
    func differenceAt(_ other: Data) -> Int {
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
    
    func fromTwoNybbleRepresentation() -> Data {
        var result = Data()
        let normalByteCount = self.count / 2  // N.B. Count must be even!
        var index = 0
        var offset = 0
        while index < normalByteCount {
            result += [UInt8.fromTwoNybbleData(data: self, at: offset)]
            offset += 2
            index += 1
        }
        return result
    }
    
    func toTwoNybbleRepresentation() -> Data {
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
    struct HexEncodingOptions: OptionSet {
        let rawValue: Int
        static let upperCase = HexEncodingOptions(rawValue: 1 << 0)
    }
    
    func hexEncodedString(options: HexEncodingOptions = []) -> String {
        let format = options.contains(.upperCase) ? "%02hhX" : "%02hhx"
        return map { String(format: format, $0) }.joined(separator: " ")
    }
    
    func decimalString() -> String {
        return map { String(format: "%02d", $0) }.joined(separator: " ")
    }
}

