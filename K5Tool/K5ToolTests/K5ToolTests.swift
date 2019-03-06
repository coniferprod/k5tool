//
//  K5ToolTests.swift
//  K5ToolTests
//
//  Created by Jere Käpyaho on 17/02/2019.
//  Copyright © 2019 Conifer Productions Oy. All rights reserved.
//

import XCTest
@testable import K5Tool

class K5ToolTests: XCTestCase {
    var testData: Data!
    
    override func setUp() {
        // Put setup code here. This method is called before the invocation of each test method in the class.
        testData = Data(bytes: rawData)
        print("Test data length = \(testData.count) bytes")
    }

    override func tearDown() {
        // Put teardown code here. This method is called after the invocation of each test method in the class.
    }

    func testExample() {
        // This is an example of a functional test case.
        // Use XCTAssert and related functions to verify your tests produce the correct results.
    }
    
    func testEmittedData() {
        do {
            let single = try Single(data: self.testData)
            let emitted = single.emit()
            let diffOffset = testData.differenceAt(emitted)
            XCTAssert(emitted == self.testData, "Emitted data should match test data exactly, differ at offset \(diffOffset)")
        } catch {
            XCTFail("Unable to construct Single from test data")
        }
    }

    func testPerformanceExample() {
        // This is an example of a performance test case.
        self.measure {
            // Put the code you want to measure the time of here.
        }
    }


}
