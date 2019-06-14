import Cocoa

class ViewController: NSViewController {

    override func viewDidLoad() {
        super.viewDidLoad()

        // Convert the data from two-nybble representation to normal bytes (04 0C --> 4C)
        let testData = Data(bytes: rawData).fromTwoNybbleRepresentation()
        //print("Test data (\(testData.count) bytes):")
        //print(testData.hexEncodedString())
        //print("Raw data (\(rawData.count) bytes):")
        //print(Data(bytes: rawData).hexEncodedString())
        
        do {
            let a = try Single(data: testData)
            let aRawData = a.emit()
            print("Emitted data (\(aRawData.count) bytes) for single A")
            print(aRawData.hexEncodedString())
            
            print("Constructing single B from emitted data of single A")
            let aCookedData = Data(bytes: aRawData).fromTwoNybbleRepresentation()
            let b = try Single(data: aCookedData)
            let bRawData = b.emit()
            print("Emitted data (\(bRawData.count) bytes) for single B")
            print(bRawData.hexEncodedString())
            
            compareSingles(a, b)
        } catch {
            print("Unable to initialize Single from SysEx data")
        }
        
        let h = Harmonic.defaultHarmonic
        let harmonicLevels = h.getHarmonicLevels(waveformName: "triangle", count: 63)
        print(harmonicLevels)
    }
    
    func columnString(_ n: String, _ aValue: Any, _ bValue: Any) -> String {
        let space: Character = " "
        let width = 20
        let nameString = n.leftPadding(toLength: width, withPad: space)
        let aValueString = "\(aValue)".leftPadding(toLength: width, withPad: space)
        let bValueString = "\(bValue)".leftPadding(toLength: width, withPad: space)
        return nameString + aValueString + bValueString
    }
    
    func compareSingles(_ a: Single, _ b: Single) {
        print(columnString("name", a.name, b.name))
        print(columnString("volume", a.volume, b.volume))
        print(columnString("balance", a.balance, b.balance))
    }

    override var representedObject: Any? {
        didSet {
        // Update the view, if already loaded.
        }
    }
}
