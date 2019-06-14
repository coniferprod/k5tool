import Foundation

typealias LeiterParameters = (a: Double, b: Double, c: Double, xp: Double, d: Double, e: Double, yp: Double)

let waveformParameters: [String: LeiterParameters] = [
    "saw": (a: 1.0, b: 0.0, c: 0.0, xp: 0.0, d: 0.0, e: 0.0, yp: 0.0),
    "square": (a: 1.0, b: 1.0, c: 0.0, xp: 0.5, d: 0.0, e: 0.0, yp: 0.0),
    "triangle": (a: 2.0, b: 1.0, c: 0.0, xp: 0.5, d: 0.0, e: 0.0, yp: 0.0)
]

extension Harmonic {
    public func leiter(n: Int, params: LeiterParameters) -> Double {
        let (a, b, c, xp, d, e, yp) = params
        
        let x = Double(n) * Double.pi * xp
        let y = Double(n) * Double.pi * yp
        
        let module1 = 1.0 / pow(Double(n), a)
        let module2 = pow(sin(x), b) * pow(cos(x), c)
        let module3 = pow(sin(y), d) * pow(cos(y), e)
        
        return module1 * module2 * module3
    }
    
    public func getHarmonicLevel(harmonic: Int, params: LeiterParameters) -> Double {
        let aMax = 1.0
        let a = leiter(n: harmonic, params: params)
        let v = log2(abs(a / aMax))
        let level = 99 + 8*v
        if level < 0 {
            return 0
        }
        return level
    }
    
    public func getHarmonicLevels(waveformName: String, count: Int) -> [Byte] {
        let params = waveformParameters[waveformName]!
        var h = [Byte]()
        var n = 0
        while n < count {
            h.append(UInt8(floor(getHarmonicLevel(harmonic: n+1, params: params))))
            n += 1
        }
        return h
    }
}
