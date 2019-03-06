import Foundation

import BinarySwift
import Bitter


// Define data types for K5 sound data model

struct EnvelopeSegment {
    var rate: Int
    var level: Int
    var isMax: Bool // not used for DFG envelopes
    var isMod: Bool // only used for DDA envelopes
    
    init(rate: Int, level: Int, isMax: Bool = false, isMod: Bool = false) {
        self.rate = rate
        self.level = level
        self.isMax = isMax
        self.isMod = isMod
    }
}

struct Envelope {
    var segments: [EnvelopeSegment]  // six segments (except DDA env has seven)
    
    init(segments: [EnvelopeSegment], isMax: Bool = false) {
        self.segments = segments
    }
    
    static var defaultEnvelope: Envelope {
        get {
            return Envelope(segments: [
                EnvelopeSegment(rate: 0, level: 0),
                EnvelopeSegment(rate: 0, level: 0),
                EnvelopeSegment(rate: 0, level: 0),
                EnvelopeSegment(rate: 0, level: 0),
                EnvelopeSegment(rate: 0, level: 0),
                EnvelopeSegment(rate: 0, level: 0)
            ])
        }
    }
}

// Use enum like a a discriminated union; either keys are tracked, or
// tracking is fixed with the given key
enum KeyTracking: Equatable {
    case track
    case fix(key: Int)  // key = 0~127
}

// The enum with associated values needed to conform to the Equatable protocol,
// and it also needed to have its own equals operator defined.

extension KeyTracking {
    static func == (lhs: KeyTracking, rhs: KeyTracking) -> Bool {
        switch lhs {
        case .track:
            switch rhs {
            case .track:
                return true
            case .fix:
                return false
            }
        case .fix(let a):
            switch rhs {
            case .fix(let b):
                return a == b
            case .track:
                return false
            }
        }
    }
}

// DHG and DDF have identical envelopes, but the segment of a harmonic envelope
// has the `isMax` setting (see K5 manual, p. 21)

struct Amplifier {
    var isActive: Bool
    var attackVelocityDepth: Int // 0~±31
    var pressureDepth: Int      // 0~±31
    var keyScalingDepth: Int    // 0~±31
    var lfoDepth: Int           // 0~31
    var attackVelocityRate: Int  // 0~±15
    var releaseVelocityRate: Int // 0~±15
    var keyScalingRate: Int      // 0~±15
    var envelope: Envelope
    
    static var defaultValues: Amplifier {
        get {
            return Amplifier(isActive: false, attackVelocityDepth: 0, pressureDepth: 0, keyScalingDepth: 0, lfoDepth: 0, attackVelocityRate: 0, releaseVelocityRate: 0, keyScalingRate: 0, envelope: Envelope.defaultEnvelope)
        }
    }
}

enum LFOShape: Int {
    case triangle = 1
    case inverseTriangle = 2
    case square = 3
    case inverseSquare = 4
    case sawtooth = 5
    case invertedSawtooth = 6
}

struct LFO {
    var shape: LFOShape
    var speed: Int // 0~99
    var delay: Int // 0~31
    var trend: Int // 0~31
    
    func emit() -> Data {
        return Data([
            UInt8(self.shape.rawValue),
            UInt8(self.speed),
            UInt8(self.delay),
            UInt8(self.trend)
        ])
    }
}

struct Filter {
    var isActive: Bool  // param 105
    var isModulationActive: Bool  // param 106
    var cutoff: Int  // 0~99
    var cutoffModulation: Int // 0~31
    var slope: Int // 0~31
    var slopeModulation: Int // 0~31
    var flatLevel: Int  // 0~31
    var velocityDepth: Int // 0~±31
    var pressureDepth: Int // 0~±31
    var keyScalingDepth: Int // 0~±31
    var envelopeDepth: Int // 0~±31
    var velocityEnvelopeDepth: Int // 0~±31
    var lfoDepth: Int  // 0~31
    
    static var defaultValues: Filter {
        get {
            return Filter(isActive: false, isModulationActive: false, cutoff: 99, cutoffModulation: 0, slope: 31, slopeModulation: 0, flatLevel: 31, velocityDepth: 0, pressureDepth: 0, keyScalingDepth: 0, envelopeDepth: 0, velocityEnvelopeDepth: 0, lfoDepth: 0)
        }
    }
}

struct Formant {
    var isActive: Bool
    var levels: [Int]   // 11 bands, corresponding to C-1 ~ C9, value 0~99
    
    func emit() -> Data {
        var d = Data()
        
        for (index, level) in self.levels.enumerated() {
            var levelValue = level
            if index == 0 {
                levelValue = levelValue.setb7(self.isActive ? 1 : 0)
            }
            
            d += [UInt8(levelValue)]
        }
        
        return d
    }
}

enum ModulationAssign: Int {
    case dfglfo = 0
    case dhg = 1
    case cutoff = 2
    case slope = 3
    case off = 4
    
    static func fromByte(_ b: UInt8) -> ModulationAssign {
        var result: ModulationAssign = .off
        switch b {
        case 0:
            result = .dfglfo
        case 1:
            result = .dhg
        case 2:
            result = .cutoff
        case 3:
            result = .slope
        case 4:
            result = .off
        default:
            result = .off
        }
        return result
    }
}

struct Harmonic {
    var level: Int   // 0~99
    var isModulationActive: Bool  // true if modulation is on for the containing source
    var envelopeNumber: Int  // user harmonic envelope number 0/1, 1/2, 2/3 or 3/4
    
    static var defaultHarmonic: Harmonic {
        get {
            return Harmonic(level: 99, isModulationActive: false, envelopeNumber: 0)
        }
    }
}

struct HarmonicEnvelopeSettings {
    var isActive: Bool
    var effect: Int  // 0~31 (SysEx manual says "s<x> env<y> off", maybe should be "eff"?)
}

enum HarmonicSelection: Int {
    case live = 0
    case die = 1
    case all = 2
}

// Harmonic modulation is either off, or on
struct HarmonicModulation {
    var isOn: Bool
    var envelopeNumber: Int
}

struct HarmonicSettings {
    var velocityDepth: Int   // 0~±31
    var pressureDepth: Int   // 0~±31
    var keyScalingDepth: Int  // 0~±31
    var lfoDepth: Int  // 0~31
    
    var envelopeSettings: [HarmonicEnvelopeSettings]
    var isModulationActive: Bool
    var selection: HarmonicSelection
    
    var rangeFrom: Int // 1~63
    var rangeTo: Int // 1~63
    
    var odd: HarmonicModulation
    var even: HarmonicModulation
    var octave: HarmonicModulation
    var fifth: HarmonicModulation
    var all: HarmonicModulation
    
    var angle: Int   // 0/-, 1/0, 1/+ (maybe should be 2/+ ?)
    var number: Int  // 1~63
    
    var isShadowOn: Bool
    
    static var defaultSettings: HarmonicSettings {
        get {
            return HarmonicSettings(velocityDepth: 0, pressureDepth: 0, keyScalingDepth: 0, lfoDepth: 0, envelopeSettings: [HarmonicEnvelopeSettings](), isModulationActive: false, selection: .all, rangeFrom: 0, rangeTo: 0, odd: HarmonicModulation(isOn: false, envelopeNumber: 1), even: HarmonicModulation(isOn: false, envelopeNumber: 1), octave: HarmonicModulation(isOn: false, envelopeNumber: 1), fifth: HarmonicModulation(isOn: false, envelopeNumber: 1), all: HarmonicModulation(isOn: false, envelopeNumber: 1), angle: 1, number: 0, isShadowOn: false)
        }
    }
}

struct KeyScaling {
    var right: Int  // 0~±31
    var left: Int   // 0~±31
    var breakpoint: Int // 0~127
    
    func emit() -> Data {
        var d = Data()
        d += [
            UInt8(bitPattern: Int8(self.right)),
            UInt8(bitPattern: Int8(self.left)),
            UInt8(self.breakpoint)
        ]
        return d
    }
}

enum PicMode: Int {
    case source1 = 0
    case source2 = 1
    case both = 2
}

let segmentCount = 6
let harmonicCount = 63
let harmonicEnvelopeCount = 4

struct Source {
    // DFG
    var coarse: Int  // 0~±48
    var fine: Int    // 0~±31
    var keyTracking: KeyTracking
    var envelopeDepth: Int // 0~±24
    var pressureDepth: Int // 0~±31
    var benderDepth: Int   // 0~24
    var velocityEnvelopeDepth: Int  // 0~±31
    var lfoDepth: Int // 0~31
    var pressureLFODepth: Int  // 0~±31
    
    var envelopeLooping: Bool
    var envelope: Envelope
    
    // DHG
    var harmonics: [Harmonic]  // there are 63 harmonics per source
    var harmonicSettings: HarmonicSettings
    var harmonicEnvelopes: [Envelope]  // four 6-segment harmonic envelopes
    
    // DDF (S381~S426)
    var filter: Filter
    var filterEnvelope: Envelope
    
    // KS
    var keyScaling: KeyScaling
    
    // DDA
    var amplifier: Amplifier
    
    init() {
        self.coarse = 0
        self.fine = 0
        self.keyTracking = .track
        self.envelopeDepth = 0
        self.pressureDepth = 0
        self.benderDepth = 0
        self.velocityEnvelopeDepth = 0
        self.lfoDepth = 0
        self.pressureLFODepth = 0
        self.envelopeLooping = false
        self.envelope = Envelope.defaultEnvelope
        self.harmonics = [Harmonic](repeating: Harmonic.defaultHarmonic, count: harmonicCount)
        self.harmonicSettings = HarmonicSettings.defaultSettings
        self.harmonicEnvelopes = [Envelope](repeating: Envelope.defaultEnvelope, count: harmonicEnvelopeCount)
        self.filter = Filter.defaultValues
        self.filterEnvelope = Envelope.defaultEnvelope
        self.amplifier = Amplifier.defaultValues
        self.keyScaling = KeyScaling(right: 0, left: 0, breakpoint: 0)
    }

    // Source-specific data is found between S11 and S468.
    // In this range, even-numbered bytes are for source 1, and odd-numbered bytes are for source 2.
    // It follows that you can take every other byte from the buffer and parse a source from it.
    // Finally, when you're done, you can fold the buffers together using something like
    //     zip(s1, s2).flatMap { [$0, $1] }
    
    // Since the source 1 and source 2 data are interspersed, we pass a source number
    // to indicate which ones to pick.
    init(data: Data, sourceNumber: Int) throws {
        print("BEGIN SOURCE \(sourceNumber)")
        
        // Get the bytes for the correct source:
        let d = stride(from: sourceNumber - 1, to: data.count, by: 2).map { data[$0] }
        //print("Reading source data for source \(sourceNumber) (\(d.count) bytes)")
        
        //let d = BinaryData(data: sourceData)
        
        var byteValue: Byte = 0
        var offset = 0
        
        // DFG
        //print("Reading DFG")
        
        // Coarse is 0~±48, so it must be two's complement
        self.coarse = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("coarse = \(self.coarse)")
        
        // Fine is the same as coarse
        self.fine = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("fine = \(self.coarse)")

        byteValue = d[offset]
        if byteValue.b7 == 1 {
            let key = Int(byteValue & 0b01111111)
            self.keyTracking = .fix(key: key)
            print("key tracking = fixed, key = \(key)")
        }
        else {
            self.keyTracking = .track
            print("key tracking = track")
        }
        offset += 1
        
        self.envelopeDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("envelope depth = \(self.envelopeDepth)")
        
        self.pressureDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("pressure depth = \(self.envelopeDepth)")

        self.benderDepth = Int(Int8(bitPattern: d[offset]))
        // FIXME: Is bender depth really 0~24 or 0~±24 ?
        offset += 1
        print("bender depth = \(self.envelopeDepth)")
        
        self.velocityEnvelopeDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("velocity envelope depth = \(self.velocityEnvelopeDepth)")

        self.lfoDepth = Int(d[offset])
        offset += 1
        print("LFO depth = \(self.lfoDepth)")

        self.pressureLFODepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("pressure LFO depth = \(self.pressureLFODepth)")

        //print("DFG envelope start, offset = \(offset) (S\(offset*2 + 21))")
        
        // Envelope looping on/off is bit7 of S39
        byteValue = d[offset]
        self.envelopeLooping = (byteValue.b7 == 1)
        print("envelope looping = \(self.envelopeLooping)")
        
        // N.B. Don't advance offset since we need to start at S39 with the segments

        // Initialize the segments so that we can fill in the rates and levels as we go along.
        var segments = [EnvelopeSegment](repeating: EnvelopeSegment(rate: 0, level: 0), count: segmentCount)
        
        var index = 0
        while index < segmentCount {
            byteValue = d[offset]
            offset += 1
            var rate = 0
            if index == 0 {  // remove the "envelope looping" bit from the first value
                rate = Int(byteValue & 0b01111111)
            }
            else {
                rate = Int(byteValue)
            }
            segments[index].rate = rate
            index += 1
        }
        
        // Another round for the levels
        index = 0
        while index < segmentCount {
            segments[index].level = Int(Int8(bitPattern: d[offset]))
            offset += 1
            index += 1
        }
        
        self.envelope = Envelope(segments: segments)
        for (si, seg) in segments.enumerated() {
            print("s\(si + 1) \(seg.rate)/\(seg.level)")
        }

        // DHG
        //print("DHG start, offset = \(offset)")

        var harmonics = [Harmonic](repeating: Harmonic.defaultHarmonic, count: harmonicCount)
        index = 0
        while index < harmonicCount {
            harmonics[index].level = Int(d[offset])
            offset += 1
            index += 1
        }
        
        //print("After harmonic levels, offset = \(offset)")
        //print("Harmonics:")

        // First handle harmonics 1...62. They are packed two to a byte.
        var harmData = [UInt8]()
        var count = 0
        while count < 31 {  // two harmonics per byte
            byteValue = d[offset]
            offset += 1
            harmData.append(byteValue & 0b00001111)
            harmData.append((byteValue & 0b11110000) >> 4)
            count += 1
        }
        //print("Read 31 bytes, got \(harmData.count) harmonic data values")
        
        for (hi, hd) in harmData.enumerated() {
            harmonics[hi].isModulationActive = (hd.b3 == 1)
            harmonics[hi].envelopeNumber = Int(hd & 0b00000011)
        }

        index = harmonicCount - 1
        
        // It appears that the harm63 settings are duplicated in the SysEx manual:
        // S251 and S252 have the same description. Need to investigate this more;
        // in the meantime, just use the first one.
        byteValue = d[offset]
        offset += 1
        harmonics[index].isModulationActive = (byteValue.b3 == 1)
        harmonics[index].envelopeNumber = Int(byteValue & 0b00000011)

        var harmonicDump = ""
        for (i, h) in harmonics.enumerated() {
            let letter = h.isModulationActive ? "Y" : "N"
            let level = String(format: "%02d", h.level)
            let number = String(format: "%2d", i + 1)
            let s = "\(number): \(level) \(letter) \(h.envelopeNumber)\n"
            harmonicDump += s
        }
        print(harmonicDump)

        // Eat the duplicate harm63 setting
        byteValue = d[offset]
        offset += 1

        self.harmonics = harmonics
        
        //print("DHG harmonic settings start, offset = \(offset)")

        var harmonicSettings = HarmonicSettings.defaultSettings

        harmonicSettings.velocityDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("harm vel depth \(harmonicSettings.velocityDepth)")
        
        harmonicSettings.pressureDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("harm press depth \(harmonicSettings.pressureDepth)")

        harmonicSettings.keyScalingDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("harm ks depth \(harmonicSettings.keyScalingDepth)")

        harmonicSettings.lfoDepth = Int(d[offset])
        offset += 1
        print("harm LFO depth \(harmonicSettings.lfoDepth)")
        
        //print("Harmonic envelope settings start, offset = \(offset)")
        harmonicSettings.envelopeSettings = [HarmonicEnvelopeSettings](repeating: HarmonicEnvelopeSettings(isActive: false, effect: 0), count: harmonicEnvelopeCount)
        index = 0
        while index < harmonicEnvelopeCount {
            byteValue = d[offset]
            offset += 1
            let hes = HarmonicEnvelopeSettings(isActive: byteValue.b7 == 1, effect: Int(byteValue & 0b00011111))
            harmonicSettings.envelopeSettings[index] = hes
            print("harm env \(index + 1) active=\(hes.isActive) effect=\(hes.effect)")
            index += 1
        }

        byteValue = d[offset]
        offset += 1
        harmonicSettings.isModulationActive = (byteValue.b7 == 1)
        print("harm mod active=\(harmonicSettings.isModulationActive)")

        var selectionValue: HarmonicSelection = .all
        switch byteValue & 0b00000011 {
        case 0:
            selectionValue = .live
        case 1:
            selectionValue = .die
        case 2:
            selectionValue = .all
        default:
            selectionValue = .all
        }
        harmonicSettings.selection = selectionValue
        print("harm sel=\(harmonicSettings.selection)")
        
        byteValue = d[offset]
        offset += 1
        harmonicSettings.rangeFrom = Int(byteValue)

        byteValue = d[offset]
        offset += 1
        harmonicSettings.rangeTo = Int(byteValue)
        
        print("harm range from=\(harmonicSettings.rangeFrom) to \(harmonicSettings.rangeTo)")

        byteValue = d[offset]
        offset += 1
        var byteValueString = String(byteValue, radix: 2).padding(toLength: 8, withPad: "0", startingAt: 0)
        //print("harm odd/even: byte = \(byteValueString)")
        
        // Harmonic envelope selections = 0/1, 1/2, 2/3, 3/4
        var modulationValue = HarmonicModulation(isOn: byteValue.b7 == 1, envelopeNumber: Int((byteValue & 0b00110000) >> 4) + 1)
        harmonicSettings.odd = modulationValue
        print("harm odd = \(harmonicSettings.odd)")
        modulationValue = HarmonicModulation(isOn: byteValue.b3 == 1, envelopeNumber: Int(byteValue & 0b00000011) + 1)
        harmonicSettings.even = modulationValue
        print("harm even = \(harmonicSettings.even)")

        byteValue = d[offset]
        offset += 1
        modulationValue = HarmonicModulation(isOn: byteValue.b7 == 1, envelopeNumber: Int((byteValue & 0b00110000) >> 4) + 1)
        harmonicSettings.octave = modulationValue
        print("harm octave = \(harmonicSettings.octave)")

        modulationValue = HarmonicModulation(isOn: byteValue.b3 == 1, envelopeNumber: Int(byteValue & 0b00000011) + 1)
        harmonicSettings.fifth = modulationValue
        print("harm fifth = \(harmonicSettings.fifth)")

        byteValue = d[offset]
        offset += 1
        modulationValue = HarmonicModulation(isOn: byteValue.b7 == 1, envelopeNumber: Int((byteValue & 0b00110000) >> 4) + 1)
        harmonicSettings.all = modulationValue
        print("harm all = \(harmonicSettings.all)")

        byteValue = d[offset]
        offset += 1
        harmonicSettings.angle = Int(byteValue)
        print("harm angle = \(harmonicSettings.angle)")
        
        byteValue = d[offset]
        offset += 1
        harmonicSettings.number = Int(byteValue)
        print("harm number = \(harmonicSettings.angle)")

        //print("DHG envelopes start, offset = \(offset) (S\(offset*2 + 21))")

        byteValue = d[offset]
        harmonicSettings.isShadowOn = (byteValue.b7 == 1)
        // don't advance the offset since we wanted the high bit of the first envelope segment value
        print("harm shadow = \(harmonicSettings.isShadowOn)")

        self.harmonicSettings = harmonicSettings
        
        var envelopes = [Envelope](repeating: Envelope.defaultEnvelope, count: harmonicEnvelopeCount)
        index = 0
        while index < harmonicEnvelopeCount {
            var segments = [EnvelopeSegment](repeating: EnvelopeSegment(rate: 0, level: 0), count: segmentCount)
            var segmentIndex = 0
            while segmentIndex < segmentCount {
                byteValue = d[offset]
                offset += 1

                let level = Int(byteValue & 0b00111111)
                let isMax = byteValue.b6 == 1
                segments[segmentIndex].level = level
                segments[segmentIndex].isMax = isMax
                segmentIndex += 1

                print("env \(index+1) seg \(segmentIndex+1) level \(level) isMax=\(isMax)")
            }
            
            segmentIndex = 0
            while segmentIndex < segmentCount {
                byteValue = d[offset]
                offset += 1

                let rate = Int(byteValue)
                segments[segmentIndex].rate = rate
                segmentIndex += 1

                print("env \(index+1) seg \(segmentIndex+1) rate \(rate)")
            }
            
            envelopes[index].segments = segments

            index += 1
        }
        self.harmonicEnvelopes = envelopes
        
        // DDF
        //print("DDF start, offset = \(offset)")

        var f = Filter.defaultValues

        byteValue = d[offset]
        offset += 1
        f.cutoff = Int(byteValue)
        print("filter cutoff = \(f.cutoff)")

        byteValue = d[offset]
        offset += 1
        f.cutoffModulation = Int(byteValue)
        print("filter cutoff mod = \(f.cutoffModulation)")

        byteValue = d[offset]
        offset += 1
        f.slope = Int(byteValue)
        print("filter slope = \(f.slope)")

        byteValue = d[offset]
        offset += 1
        f.slopeModulation = Int(byteValue)
        print("filter slope modulation = \(f.slopeModulation)")

        byteValue = d[offset]
        offset += 1
        f.flatLevel = Int(byteValue)
        print("filter flat level = \(f.flatLevel)")

        byteValue = d[offset]
        offset += 1
        f.velocityDepth = Int(Int8(bitPattern: byteValue))
        print("filter velocity depth = \(f.velocityDepth)")

        byteValue = d[offset]
        offset += 1
        f.pressureDepth = Int(Int8(bitPattern: byteValue))
        print("filter pressure depth = \(f.pressureDepth)")

        byteValue = d[offset]
        offset += 1
        f.keyScalingDepth = Int(Int8(bitPattern: byteValue))
        print("filter key scaling depth = \(f.keyScalingDepth)")

        byteValue = d[offset]
        offset += 1
        f.envelopeDepth = Int(Int8(bitPattern: byteValue))
        print("filter envelope depth = \(f.envelopeDepth)")

        byteValue = d[offset]
        offset += 1
        f.velocityEnvelopeDepth = Int(Int8(bitPattern: byteValue))
        print("filter velocity envelope depth = \(f.velocityEnvelopeDepth)")

        byteValue = d[offset]
        offset += 1
        f.isActive = (byteValue.b7 == 1)
        f.isModulationActive = (byteValue.b6 == 1)
        f.lfoDepth = Int(byteValue & 0b00011111)
        print("filter active = \(f.isActive), filter mod active = \(f.isModulationActive)")
        print("filter LFO depth = \(f.lfoDepth)")

        self.filter = f

        var fenv = Envelope.defaultEnvelope
        var filterEnvelopeSegments = [EnvelopeSegment](repeating: EnvelopeSegment(rate: 0, level: 0), count: segmentCount)
        index = 0
        while index < segmentCount {
            print("filter env seg \(index+1) rate, offset = \(offset)")

            byteValue = d[offset]
            offset += 1
            filterEnvelopeSegments[index].rate = Int(byteValue)
            index += 1
        }
        
        index = 0
        while index < segmentCount {
            print("filter env seg \(index+1) level/ismax, offset = \(offset)")
            byteValue = d[offset]
            offset += 1
            filterEnvelopeSegments[index].isMax = (byteValue.b6 == 1)
            filterEnvelopeSegments[index].level = Int(byteValue & 0b00111111)

            index += 1
        }
        fenv.segments = filterEnvelopeSegments
        
        self.filterEnvelope = fenv

        var amp = Amplifier(isActive: false, attackVelocityDepth: 0, pressureDepth: 0, keyScalingDepth: 0, lfoDepth: 0, attackVelocityRate: 0, releaseVelocityRate: 0, keyScalingRate: 0, envelope: Envelope.defaultEnvelope)
        
        byteValue = d[offset]
        offset += 1
        amp.attackVelocityDepth = Int(Int8(bitPattern: byteValue))
        print("amp attack vel depth = \(amp.attackVelocityDepth)")
        
        byteValue = d[offset]
        offset += 1
        amp.pressureDepth = Int(Int8(bitPattern: byteValue))
        print("amp pressure depth = \(amp.pressureDepth)")

        byteValue = d[offset]
        offset += 1
        amp.keyScalingDepth = Int(Int8(bitPattern: byteValue))
        print("amp ks depth = \(amp.keyScalingDepth)")

        byteValue = d[offset]
        offset += 1
        amp.isActive = (byteValue.b7 == 1)
        amp.lfoDepth = Int(byteValue & 0b01111111)
        print("amp active=\(amp.isActive) LFO depth \(amp.lfoDepth)")
        
        byteValue = d[offset]
        offset += 1
        amp.attackVelocityRate = Int(Int8(bitPattern: byteValue))
        print("amp attack vel rate = \(amp.attackVelocityRate)")

        byteValue = d[offset]
        offset += 1
        amp.releaseVelocityRate = Int(Int8(bitPattern: byteValue))
        print("amp attack vel rate = \(amp.releaseVelocityRate)")

        byteValue = d[offset]
        offset += 1
        amp.keyScalingRate = Int(Int8(bitPattern: byteValue))
        print("amp ks rate = \(amp.keyScalingRate)")

        var ampEnvSeg = [EnvelopeSegment](repeating: EnvelopeSegment(rate: 0, level: 0), count: segmentCount + 1)  // one extra segment for DDA ENV
        index = 0
        while index < ampEnvSeg.count {
            byteValue = d[offset]
            offset += 1
            ampEnvSeg[index].isMod = (byteValue.b6 == 1)
            ampEnvSeg[index].rate = Int(byteValue & 0b00111111)
            print("amp env seg \(index + 1) rate = \(ampEnvSeg[index].rate) isMod=\(ampEnvSeg[index].isMod)")

            index += 1
        }
        
        index = 0
        while index < segmentCount {
            byteValue = d[offset]
            offset += 1

            ampEnvSeg[index].isMax = (byteValue.b6 == 1)
            ampEnvSeg[index].level = Int(byteValue & 0b00111111)
            print("amp env seg \(index + 1) level = \(ampEnvSeg[index].level) isMax=\(ampEnvSeg[index].isMax)")

            index += 1
        }

        amp.envelope = Envelope(segments: ampEnvSeg)
        
        self.amplifier = amp
        
        var ks = KeyScaling(right: 0, left: 0, breakpoint: 0)
        byteValue = d[offset]
        offset += 1
        ks.right = Int(Int8(bitPattern: byteValue))
        
        byteValue = d[offset]
        offset += 1
        ks.left = Int(Int8(bitPattern: byteValue))
        
        byteValue = d[offset]
        offset += 1
        ks.breakpoint = Int(byteValue)
        
        print("ks right=\(ks.right) left=\(ks.left) bp=\(ks.breakpoint)")
        
        self.keyScaling = ks
        
        print("END SOURCE \(sourceNumber)")
    }
    
    // Emit SysEx representation
    func emit() -> Data {
        var d = Data()
        
        // coarse and fine
        d += [
            UInt8(bitPattern: Int8(self.coarse)),
            UInt8(bitPattern: Int8(self.fine))
        ]
        
        var keyTrackingValue: UInt8 = 0
        switch self.keyTracking {
        case .track:
            keyTrackingValue = 0
        case .fix(let key):
            keyTrackingValue = UInt8(key)
            keyTrackingValue = keyTrackingValue.setb7(1)
        }
        d += [keyTrackingValue]
        
        d += [
            UInt8(bitPattern: Int8(self.envelopeDepth)),
            UInt8(bitPattern: Int8(self.pressureDepth)),
            UInt8(bitPattern: Int8(self.benderDepth)),
            UInt8(bitPattern: Int8(self.velocityEnvelopeDepth)),
            UInt8(self.lfoDepth),
            UInt8(bitPattern: Int8(self.pressureLFODepth))
        ]
        
        // DFG envelope
        for (index, segment) in self.envelope.segments.enumerated() {
            var rateValue = segment.rate
            if index == 0 && self.envelopeLooping {
                rateValue = rateValue.setb7(1)
            }
            d += [UInt8(rateValue)]
        }
        
        for segment in self.envelope.segments {
            d += [UInt8(bitPattern: Int8(segment.level))]
        }
        print("after DFG envelope, have \(d.count) bytes")
        
        //
        // DHG
        //
        for h in self.harmonics {
            d += [UInt8(h.level)]
        }
        print("after DHG levels, have \(d.count) bytes")
        
        var isActive = false
        var number = 0
        var harmValue: UInt8 = 0
        
        // First handle harmonics 1...62. They are packed two to a byte.
        var harmNum = 0
        var byteCount = 0
        while byteCount < 31 {
            let h1 = self.harmonics[harmNum]
            harmNum += 1
            var value = UInt8(h1.envelopeNumber)
            value = value.setb3(h1.isModulationActive ? 1 : 0)
            let h2 = self.harmonics[harmNum]
            harmNum += 1
            value = value | (UInt8(h2.envelopeNumber) << 4)
            value = value.setb7(h2.isModulationActive ? 1 : 0)
            byteCount += 1
            d += [value]
        }
        
        // Handle the last harmonic (63rd)
        let harm = self.harmonics[62]
        isActive = harm.isModulationActive
        number = harm.envelopeNumber
        harmValue = UInt8(number)
        if isActive {
            harmValue = harmValue.setb3(1)
        }
        d += [UInt8(harmValue)]
        
        // Duplicate the 63rd harmonic for now (see SysEx parser for details)
        d += [UInt8(harmValue)]
        print("after DHG harmonic selection, have \(d.count) bytes")
        
        let harmSet = self.harmonicSettings
        d += [
            UInt8(bitPattern: Int8(harmSet.velocityDepth)),
            UInt8(bitPattern: Int8(harmSet.pressureDepth)),
            UInt8(bitPattern: Int8(harmSet.keyScalingDepth)),
            UInt8(harmSet.lfoDepth)
        ]
        print("after DHG harmonic settings, have \(d.count) bytes")

        for (hi, he) in harmSet.envelopeSettings.enumerated() {
            print("HE\(hi): active = \(he.isActive), eff = \(he.effect)")
            var effectValue = he.effect
            effectValue = effectValue.setb7(he.isActive ? 1 : 0)
            d += [UInt8(effectValue)]
        }
        print("after DHG harmonic envelope settings, have \(d.count) bytes")

        // harmonic modulation
        isActive = harmSet.isModulationActive
        var harmSel = UInt8(harmSet.selection.rawValue)
        harmSel = harmSel.setb7(isActive ? 1 : 0)
        d += [UInt8(harmSel)]
        
        d += [
            UInt8(harmSet.rangeFrom),
            UInt8(harmSet.rangeTo)
        ]
        
        // Harmonic envelope selections = 0/1, 1/2, 2/3, 3/4
        harmSel = UInt8(harmSet.odd.envelopeNumber - 1) << 4
        harmSel = harmSel.setb7(harmSet.odd.isOn ? 1 : 0)
        harmSel = harmSel | UInt8(harmSet.even.envelopeNumber - 1)
        harmSel = harmSel.setb3(harmSet.even.isOn ? 1 : 0)
        d += [UInt8(harmSel)]
        
        harmSel = UInt8(harmSet.octave.envelopeNumber - 1) << 4
        harmSel = harmSel.setb7(harmSet.octave.isOn ? 1 : 0)
        harmSel = harmSel | UInt8(harmSet.fifth.envelopeNumber - 1)
        harmSel = harmSel.setb3(harmSet.fifth.isOn ? 1 : 0)
        d += [UInt8(harmSel)]
        
        harmSel = UInt8(harmSet.all.envelopeNumber - 1) << 4
        harmSel = harmSel.setb7(harmSet.all.isOn ? 1 : 0)
        d += [UInt8(harmSel)]
        
        d += [
            UInt8(harmSet.angle),
            UInt8(harmSet.number)
        ]
        print("after harmonic settings, have \(d.count) bytes")
        
        for (ei, env) in self.harmonicEnvelopes.enumerated() {
            let segments = env.segments
            
            for (si, seg) in segments.enumerated() {
                var levelValue = seg.level
                levelValue = levelValue.setb6(seg.isMax ? 1 : 0)
                print("env\(ei + 1) seg \(si + 1) level \(seg.level) isMax=\(seg.isMax)")

                // Handle special case in first value
                if ei == 0 && si == 0 {
                    levelValue = levelValue.setb7(harmSet.isShadowOn ? 1 : 0)
                }
                
                d += [UInt8(levelValue)]
            }
            
            for (si, seg) in segments.enumerated() {
                print("env\(ei + 1) seg \(si + 1) rate \(seg.rate)")
                d += [UInt8(seg.rate)]
            }
        }
        print("after harmonic envelopes, have \(d.count) bytes")
        
        d += [
            UInt8(self.filter.cutoff),
            UInt8(self.filter.cutoffModulation),
            UInt8(self.filter.slope),
            UInt8(self.filter.slopeModulation),
            UInt8(self.filter.flatLevel),
            UInt8(bitPattern: Int8(self.filter.velocityDepth)),
            UInt8(bitPattern: Int8(self.filter.pressureDepth)),
            UInt8(bitPattern: Int8(self.filter.keyScalingDepth)),
            UInt8(bitPattern: Int8(self.filter.envelopeDepth)),
            UInt8(bitPattern: Int8(self.filter.velocityEnvelopeDepth))
        ]
        
        var filterValue = UInt8(self.filter.lfoDepth)
        filterValue = filterValue.setb7(self.filter.isActive ? 1 : 0)
        filterValue = filterValue.setb6(self.filter.isModulationActive ? 1 : 0)
        d += [filterValue]
        
        // Filter envelope
        for (si, seg) in self.filterEnvelope.segments.enumerated() {
            print("f.env. seg \(si + 1) rate \(seg.rate)")
            d += [UInt8(seg.rate)]
        }
        
        for (si, seg) in self.filterEnvelope.segments.enumerated() {
            var levelValue = seg.level
            levelValue = levelValue.setb6(seg.isMax ? 1 : 0)
            print("flt env seg \(si + 1) level \(seg.level) isMax=\(seg.isMax)")
            d += [UInt8(levelValue)]
        }

        print("after filter, have \(d.count) bytes")
        
        let amp = self.amplifier
        
        d += [
            UInt8(bitPattern: Int8(amp.attackVelocityDepth)),
            UInt8(bitPattern: Int8(amp.pressureDepth)),
            UInt8(bitPattern: Int8(amp.keyScalingDepth))
        ]
        
        var lfoDepthValue = amp.lfoDepth
        lfoDepthValue = lfoDepthValue.setb7(amp.isActive ? 1 : 0)
        d += [UInt8(lfoDepthValue)]
        
        d += [
            UInt8(bitPattern: Int8(amp.attackVelocityRate)),
            UInt8(bitPattern: Int8(amp.releaseVelocityRate)),
            UInt8(bitPattern: Int8(amp.keyScalingRate)),
        ]
        
        // Amp envelope. N.B. This envelope has seven rates but only six levels
        for (si, seg) in amp.envelope.segments.enumerated() {
            var rateValue = seg.rate
            rateValue = rateValue.setb6(seg.isMod ? 1 : 0)
            print("amp env seg \(si + 1) rate \(seg.rate) mod=\(seg.isMod)")
            d += [UInt8(seg.rate)]
        }
        
        for (si, seg) in amp.envelope.segments.enumerated() {
            var levelValue = seg.level
            levelValue = levelValue.setb6(seg.isMax ? 1 : 0)
            if si == 6 {
                print("no level for amp env seg 7")
                d += [UInt8(0)]
            }
            else {
                print("amp env seg \(si + 1) level \(seg.level) isMaxSeg=\(seg.isMax)")
                d += [UInt8(levelValue)]
            }
        }

        print("after amp, have \(d.count) bytes")

        // NOTE: Don't emit the key scaling data here. See Single.emit().
        
        return d
    }
}


enum SourceMode: Int {
    case twin = 0
    case full = 1
}

// TODO: Construct a CharacterSet with just the characters allowed in a name:
// (0~9, A~Z, -, :, /, *, ?, !, #, &, (, ), ", +, ., =, space)

let nameLength = 8   // characters, not bytes

struct Single {
    var name: String  // max 8 characters (S1~S8)
    var volume: Int   // 0~63
    var balance: Int  // 0~±31
    
    // S11~S18
    var source1Delay: Int  // 0~31
    var source2Delay: Int  // 0~31
    var source1PedalDepth: Int     // 0~31
    var source2PedalDepth: Int     // 0~31
    var source1WheelDepth: Int     // 0~31
    var source2WheelDepth: Int     // 0~31
    var source1PedalAssign: ModulationAssign
    var source2PedalAssign: ModulationAssign
    var source1WheelAssign: ModulationAssign
    var source2WheelAssign: ModulationAssign
    
    var portamento: Bool
    var portamentoSpeed: Int       // 0~63
    var mode: SourceMode  // called "s2 mode" in the SysEx manual
    var picMode: PicMode  // TODO: What is the "pic mode" setting? "0/s1, 1/s2, 2/both"?
    
    // DFG
    var source1: Source
    var source2: Source
    
    // LFO
    var lfo: LFO
    
    // DFT
    var formant: Formant
    
    var sum: UInt16  // the Kawai checksum
    
    let verbose = true
    
    func log(_ s: String) {
        if verbose {
            print(s)
        }
    }
    
    init() {
        self.name = "INIT"
        self.volume = 63
        self.balance = 0
        
        self.source1Delay = 0
        self.source2Delay = 0
        self.source1PedalDepth = 0
        self.source2PedalDepth = 0
        self.source1WheelDepth = 0
        self.source2WheelDepth = 0
        self.source1PedalAssign = .off
        self.source2PedalAssign = .off
        self.source1WheelAssign = .off
        self.source2WheelAssign = .off

        self.portamento = false
        self.portamentoSpeed = 63
        self.mode = .twin
        self.picMode = .source1
        self.source1 = Source()
        self.source2 = Source()
        self.lfo = LFO(shape: .triangle, speed: 0, delay: 0, trend: 0)
        self.formant = Formant(isActive: false, levels: [Int](repeating: 63, count: 11))
        
        self.sum = 0
    }
    
    // Initialize the Single from SysEx data (minus SysEx header and terminator)
    init(data: Data) throws {
        //let d = BinaryData(data: data)  // defaults to Big Endian
        let d = data
        //print("Reading single data (\(d.count) bytes)")
        print("BEGIN SINGLE")
        
        // Define some scratch variables to store the nybbles that make up a one-byte value
        var byteValue: Byte = 0
        var b: Byte = 0
        
        // name = S1 ... S8
        var offset = 0
        var name = ""
        var charCount = 0
        while charCount < nameLength {
            b = d[offset]
            if let scalarValue = UnicodeScalar(UInt16(b)) {
                let s = String(scalarValue)
                name += s
            }
            offset += 1
            charCount += 1
        }
        self.name = name
        print("name = '\(self.name)'")

        // volume - S9
        self.volume = Int(d[offset])
        print("volume = \(self.volume)")
        offset += 1
        
        // balance - S10
        self.balance = Int(Int8(bitPattern: d[offset]))
        print("balance = \(self.balance)")
        offset += 1
        
        // Pick up the modulation values first
        self.source1Delay = Int(d[offset])
        offset += 1
        print("s1 env delay = \(self.source1Delay)")

        self.source2Delay = Int(d[offset])
        offset += 1
        print("s2 env delay = \(self.source2Delay)")

        self.source1PedalDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("s1 pedal depth = \(self.source1PedalDepth)")

        self.source2PedalDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("s2 pedal depth = \(self.source2PedalDepth)")

        self.source1WheelDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("s1 wheel depth = \(self.source1WheelDepth)")

        self.source2WheelDepth = Int(Int8(bitPattern: d[offset]))
        offset += 1
        print("s2 wheel depth = \(self.source2WheelDepth)")

        // pedal assign and wheel assign are packed into one byte for s1 and s2
        byteValue = d[offset]
        var pedal = (byteValue & 0b11110000) >> 4
        self.source1PedalAssign = ModulationAssign.fromByte(pedal)
        print("s1 pedal assign = \(self.source1PedalAssign)")

        var wheel = byteValue & 0b00001111
        self.source1WheelAssign = ModulationAssign.fromByte(wheel)
        print("s1 wheel assign = \(self.source1WheelAssign)")

        offset += 1

        byteValue = d[offset]
        pedal = (byteValue & 0b11110000) >> 4
        self.source2PedalAssign = ModulationAssign.fromByte(pedal)
        print("s2 pedal assign = \(self.source2PedalAssign)")

        wheel = byteValue & 0b00001111
        self.source2WheelAssign = ModulationAssign.fromByte(wheel)
        print("s1 wheel assign = \(self.source2WheelAssign)")

        offset += 1
        
        // portamento and p. speed - S19
        byteValue = d[offset]
        self.portamento = (byteValue.b7 == 1)
        self.portamentoSpeed = Int(byteValue & 0b00111111)
        print("portamento = \(self.portamento), speed = \(self.portamentoSpeed)")
        offset += 1
        
        // mode - S20
        byteValue = d[offset]
        self.mode = (byteValue.b2 == 1) ? .full : .twin
        let picModeValue = byteValue & 0b00000011
        var picMode: PicMode = .both
        switch picModeValue {
        case 0:
            picMode = .source1
        case 1:
            picMode = .source2
        case 2:
            picMode = .both
        default:
            picMode = .both
        }
        self.picMode = picMode
        print("s2 mode = \(self.mode), pic mode = \(self.picMode)")
        offset += 1
        
        //print("s1 start, offset = \(offset)")
        let sourceData = d.subdata(in: offset..<d.count)
        //print("sourceData.count = \(sourceData.count)")
        self.source1 = try Source(data: sourceData, sourceNumber: 1)

        //print("s2 start, offset = \(offset)")
        self.source2 = try Source(data: sourceData, sourceNumber: 2)
        
        offset = 468
        
        //print("LFO start, offset = \(offset)")
        var theLFO = LFO(shape: .triangle, speed: 0, delay: 0, trend: 0)
        
        byteValue = d[offset]
        offset += 1
        theLFO.shape = LFOShape(rawValue: Int(byteValue)) ?? .triangle
        
        byteValue = d[offset]
        offset += 1
        theLFO.speed = Int(byteValue)
        
        byteValue = d[offset]
        offset += 1
        theLFO.delay = Int(byteValue)
        
        byteValue = d[offset]
        offset += 1
        theLFO.trend = Int(byteValue)
        
        self.lfo = theLFO
        //print("after LFO, offset = \(offset)")
        print("LFO shape=\(theLFO.shape) speed=\(theLFO.speed) delay=\(theLFO.delay) trend=\(theLFO.trend)")
        
        var theFormant = Formant(isActive: false, levels: [Int](repeating: 63, count: 11))
        var formantLevels = [Int](repeating: 63, count: 11)
        var index = 0
        while index < 11 {
            byteValue = d[offset]
            offset += 1

            if index == 0 {
                theFormant.isActive = (byteValue.b7 == 1)
            }
            
            formantLevels[index] = Int(byteValue & 0b01111111)
            
            index += 1
        }
        theFormant.levels = formantLevels
        self.formant = theFormant
        //print("after formant, offset = \(offset)")
        print("formant active=\(theFormant.isActive)")
        for (formantIndex, formantLevel) in self.formant.levels.enumerated() {
            print("formant level \(formantIndex + 1) = \(formantLevel)")
        }
        
        byteValue = d[offset]
        offset += 1
        let sumLow = byteValue
        
        byteValue = d[offset]
        offset += 1
        let sumHigh = byteValue
        
        self.sum = (UInt16(sumHigh) << 8) | UInt16(sumLow)
        let checksumString = String(format: "%04X", self.sum)
        print("checksum = \(checksumString)")
        
        print("END SINGLE")
    }
    
    private func emitName() -> Data {
        var d = Data()
        
        for value in self.name.ascii {
            d += [UInt8(value)]
        }

        return d
    }
    
    public func emit() -> Data {
        var d = Data()
        
        d += emitName()
        d += [
            UInt8(self.volume),
            UInt8(bitPattern: Int8(self.balance)),
            UInt8(self.source1Delay),
            UInt8(self.source2Delay),
            UInt8(bitPattern: Int8(self.source1PedalDepth)),
            UInt8(bitPattern: Int8(self.source2PedalDepth)),
            UInt8(bitPattern: Int8(self.source1WheelDepth)),
            UInt8(bitPattern: Int8(self.source2WheelDepth)),
            UInt8(self.source1PedalAssign.rawValue) << 4 | UInt8(self.source1WheelAssign.rawValue),
            UInt8(self.source2PedalAssign.rawValue) << 4 | UInt8(self.source2WheelAssign.rawValue)
        ]

        // We have a choice of emitting the SysEx values in order,
        // so that we pick whatever we need from the single as we go along,
        // or we systematically emit the single components and use random
        // access to the data. We choose the first option, so we can just
        // cruise along and emit, without maintaining offsets.
        
        // portamento and p. speed
        var portamentoByteValue = UInt8(self.portamentoSpeed)
        if self.portamento {
            portamentoByteValue = portamentoByteValue.setb7(1)
        }
        d += [portamentoByteValue]
        
        // source mode and "pic mode" (whatever that is)
        let modeValue = UInt8(self.picMode.rawValue) | (UInt8(self.mode.rawValue) << 2)
        d += [modeValue]

        //print("before source, have \(d.count) bytes")

        let s1 = self.source1.emit()
        //print("emitted \(s1.count) bytes for s1")
        let s2 = self.source2.emit()
        //print("emitted \(s2.count) bytes for s1")

        // Interleave the source 1 and source 2 data
        d += zip(s1, s2).flatMap { [$0, $1] }
        //print("after interleaving s1 and s2, have \(d.count) bytes")

        d += self.lfo.emit()
        //print("after LFO, have \(d.count) bytes")

        // N.B. The emitted source data does not have the source-specific
        // key scaling data. Instead we pick it up here, so that we can
        // emit the whole SysEx linearly.
        let ks1 = self.source1.keyScaling.emit()
        let ks2 = self.source2.keyScaling.emit()
        let ksdata = zip(ks1, ks2).flatMap { [$0, $1] }
        print("ks data is \(ksdata.count) bytes")
        d += ksdata
        //print("after interleaving ks1 and ks2, have \(d.count) bytes")

        d += self.formant.emit()
        //print("after formant, have \(d.count) bytes")
        
        d += [UInt8(0)]  // S490
        
        // emit checksum
        let sum = checksum(data: d)
        d += [
            UInt8(sum & 0xFF),
            UInt8((sum >> 8) & 0xFF)
        ]
        let sumString = String(format: "%04X", sum)
        print("checksum = \(sumString)")
        
        //print("ready to convert \(d.count) bytes into two-nybble representation")
        let dn = d.toTwoNybbleRepresentation()
        //print(dn.hexEncodedString())
        return dn
    }
    
    // Compute the Kawai K5 checksum from the data
    func checksum(data: Data) -> UInt16 {
        var sum: UInt16 = 0
        for i in stride(from: 0, to: data.count, by: 2) {
            sum += UInt16((((data[i + 1] & 0xFF) << 8) | (data[i] & 0xFF)))
            
        }
        
        sum = sum & 0xFFFF
        sum = (0x5A3C - sum) & 0xFFFF
        
        return sum
    }
}

typealias LeiterParameters = (a: Double, b: Double, c: Double, xp: Double, d: Double, e: Double, yp: Double)

let waveformParameters: [String: LeiterParameters] = [
    "saw": (a: 1.0, b: 0.0, c: 0.0, xp: 0.0, d: 0.0, e: 0.0, yp: 0.0),
    "square": (a: 1.0, b: 1.0, c: 0.0, xp: 0.5, d: 0.0, e: 0.0, yp: 0.0),
    "triangle": (a: 2.0, b: 1.0, c: 0.0, xp: 0.5, d: 0.0, e: 0.0, yp: 0.0)
]

extension Harmonic {
    func leiter(n: Int, params: LeiterParameters) -> Double {
        let (a, b, c, xp, d, e, yp) = params
    
        let x = Double(n) * Double.pi * xp
        let y = Double(n) * Double.pi * yp
    
        let module1 = 1.0 / pow(Double(n), a)
        let module2 = pow(sin(x), b) * pow(cos(x), c)
        let module3 = pow(sin(y), d) * pow(cos(y), e)
    
        return module1 * module2 * module3
    }
    
    func getHarmonicLevel(harmonic: Int, params: LeiterParameters) -> Double {
        let aMax = 1.0
        let a = leiter(n: harmonic, params: params)
        let v = log2(abs(a / aMax))
        let level = 99 + 8*v
        if level < 0 {
            return 0
        }
        return level
    }

    func getHarmonicLevels(waveformName: String, count: Int) -> [UInt8] {
        let params = waveformParameters[waveformName]!
        var h = [UInt8]()
        var n = 0
        while n < count {
            h.append(UInt8(floor(getHarmonicLevel(harmonic: n+1, params: params))))
            n += 1
        }
        return h
    }
}

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

typealias Byte = UInt8