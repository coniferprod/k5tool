//
//  TestData.swift
//  K5Tool
//
//  Created by Jere Käpyaho on 18/02/2019.
//  Copyright © 2019 Conifer Productions Oy. All rights reserved.
//

import Foundation

public let rawData: [UInt8] = [
    0x04, 0x0c, 0x04, 0x0f, 0x05, 0x07, 0x05, 0x03,  // 4C 4F 57 53
    0x05, 0x04, 0x05, 0x02, 0x05, 0x03, 0x03, 0x01,  // 54 52 53 31
    0x03, 0x0f,  // volume = 3Fh = 63
    0x0f, 0x0b,  // balance = FBh
    0x00, 0x00,  // s1 delay
    0x00, 0x00,  // s2 delay
    0x00, 0x00,  // s1 pedal dep
    0x00, 0x00,  // s2 pedal dep
    0x0f, 0x0a,  // s1 wheel dep
    0x00, 0x06,  // s2 wheel dep
    0x00, 0x00,  // s1 pedal assign + wheel assign
    0x00, 0x00,  // s2 pedal assign + wheel assign
    0x00, 0x0a,  // portamento: b7 is flag, b0-6 are speed
    0x00, 0x02,  // s2 mode and pic mode
    0x0f, 0x04,  // s1 coarse
    0x0f, 0x04,  // s2 coarse
    0x00, 0x00, 0x0f, 0x06, 0x04, 0x00, 0x04, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
    0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00,
    0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00,
    0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f,
    0x00, 0x00, 0x00, 0x00, 0x0e, 0x01, 0x0e, 0x01, 0x00, 0x00, 0x00, 0x00,
    0x06, 0x03, 0x06, 0x03, 0x04, 0x0f, 0x05, 0x07, 0x04, 0x05, 0x04, 0x0c,
    0x03, 0x0f, 0x05, 0x00, 0x03, 0x0a, 0x04, 0x06, 0x03, 0x06, 0x04, 0x00,
    0x03, 0x03, 0x04, 0x04, 0x03, 0x00, 0x04, 0x06, 0x02, 0x0e, 0x04, 0x06,
    0x02, 0x0c, 0x03, 0x09, 0x02, 0x09, 0x02, 0x03, 0x02, 0x08, 0x02, 0x01,
    0x02, 0x06, 0x02, 0x07, 0x02, 0x04, 0x03, 0x01, 0x02, 0x04, 0x03, 0x02,
    0x02, 0x01, 0x02, 0x0b, 0x02, 0x01, 0x02, 0x09, 0x02, 0x00, 0x02, 0x0d,
    0x01, 0x0e, 0x02, 0x08, 0x01, 0x0e, 0x02, 0x06, 0x01, 0x0c, 0x02, 0x08,
    0x01, 0x0b, 0x02, 0x06, 0x01, 0x0b, 0x02, 0x02, 0x01, 0x09, 0x02, 0x07,
    0x01, 0x09, 0x02, 0x07, 0x01, 0x08, 0x02, 0x03, 0x01, 0x06, 0x02, 0x04,
    0x01, 0x06, 0x02, 0x04, 0x01, 0x03, 0x02, 0x03, 0x01, 0x01, 0x02, 0x04,
    0x01, 0x01, 0x02, 0x04, 0x00, 0x0c, 0x02, 0x02, 0x00, 0x0d, 0x01, 0x0f,
    0x00, 0x0b, 0x01, 0x0d, 0x00, 0x09, 0x01, 0x0b, 0x00, 0x0d, 0x01, 0x0a,
    0x00, 0x08, 0x01, 0x09, 0x00, 0x0b, 0x01, 0x08, 0x00, 0x0b, 0x01, 0x09,
    0x00, 0x08, 0x01, 0x09, 0x00, 0x0c, 0x01, 0x0a, 0x00, 0x09, 0x01, 0x09,
    0x00, 0x0a, 0x01, 0x0c, 0x00, 0x0c, 0x01, 0x0c, 0x00, 0x08, 0x01, 0x0a,
    0x00, 0x0c, 0x01, 0x0c, 0x00, 0x0b, 0x01, 0x0c, 0x00, 0x09, 0x01, 0x0b,
    0x00, 0x0d, 0x01, 0x0b, 0x00, 0x09, 0x01, 0x0a, 0x00, 0x0b, 0x01, 0x0a,
    0x00, 0x0b, 0x01, 0x0c, 0x00, 0x07, 0x01, 0x0e, 0x00, 0x0b, 0x02, 0x00,
    0x00, 0x08, 0x02, 0x01, 0x00, 0x06, 0x02, 0x00, 0x00, 0x09, 0x01, 0x0f,
    0x00, 0x03, 0x02, 0x00, 0x00, 0x05, 0x02, 0x01, 0x00, 0x04, 0x02, 0x02,
    0x00, 0x00, 0x02, 0x01, 0x00, 0x03, 0x02, 0x00, 0x00, 0x00, 0x01, 0x0c,
    0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x08, 0x00, 0x09, 0x0f, 0x08, 0x02, 0x08, 0x02, 0x00, 0x01, 0x00, 0x01,
    0x03, 0x0f, 0x03, 0x0f, 0x00, 0x09, 0x00, 0x08, 0x0a, 0x0b, 0x0a, 0x08,
    0x08, 0x00, 0x08, 0x00, 0x00, 0x01, 0x00, 0x01, 0x01, 0x0d, 0x03, 0x0f,
    0x09, 0x0f, 0x09, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f,
    0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00,
    0x01, 0x01, 0x01, 0x01, 0x01, 0x05, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00,
    0x05, 0x09, 0x05, 0x09, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f,
    0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00,
    0x00, 0x0e, 0x00, 0x0e, 0x01, 0x05, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00,
    0x05, 0x09, 0x05, 0x09, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f,
    0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00,
    0x00, 0x0e, 0x00, 0x0e, 0x01, 0x05, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00,
    0x05, 0x09, 0x05, 0x09, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f,
    0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x00,
    0x00, 0x0e, 0x00, 0x0e, 0x01, 0x05, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00,
    0x04, 0x00, 0x04, 0x00, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x03, 0x01, 0x03,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x03, 0x0c, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x08, 0x00, 0x08, 0x00, 0x0d, 0x00, 0x0d, 0x00, 0x0f, 0x00, 0x0f,
    0x00, 0x0a, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x09, 0x01, 0x09,
    0x01, 0x0f, 0x01, 0x0f, 0x00, 0x0c, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x01, 0x03, 0x01, 0x06, 0x01, 0x07, 0x01, 0x07, 0x01, 0x06, 0x01, 0x06,
    0x01, 0x05, 0x01, 0x05, 0x01, 0x03, 0x01, 0x03, 0x00, 0x0b, 0x00, 0x0b,
    0x00, 0x00, 0x00, 0x00, 0x01, 0x09, 0x01, 0x09, 0x01, 0x0f, 0x01, 0x0f,
    0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x01, 0x0f, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x03, 0x0e,
    0x00, 0x00, 0x00, 0x0a, 0x00, 0x06, 0x00, 0x06, 0x0f, 0x09, 0x0f, 0x09,
    0x03, 0x08, 0x03, 0x08, 0x03, 0x0b, 0x03, 0x0b, 0x03, 0x0b, 0x03, 0x0b,
    0x03, 0x0b, 0x03, 0x0f, 0x03, 0x0c, 0x03, 0x0d, 0x03, 0x0d, 0x03, 0x0b,
    0x03, 0x0b, 0x00, 0x00, 0x04, 0x04, 0x08, 0x03
]