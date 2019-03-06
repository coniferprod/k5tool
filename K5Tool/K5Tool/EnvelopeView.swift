//
//  EnvelopeView.swift
//  K5Tool
//
//  Created by Jere Käpyaho on 21/02/2019.
//  Copyright © 2019 Conifer Productions Oy. All rights reserved.
//

import Foundation
import AppKit

public class EnvelopeView: NSView {
    var envelope: Envelope

    var areaPaths: [NSBezierPath]  // one for each segment
    
    public init(frame: CGRect = CGRect(origin: 440, size: 150), envelope: Envelope) {
        self.envelope = envelope
        
        super.init(frame: frame)
    }
    
    required public init?(coder aDecoder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
    
    override public func draw(_ rect: CGRect) {
        _ = NSGraphicsContext.current?.cgContext
        
        let backgroundColor = NSColor.white
        
        self.wantsLayer = true
        self.layer?.backgroundColor = backgroundColor.cgColor
        
        areaPaths.removeAll()
        for segment in self.envelope.segments {
            areaPaths.append(NSBezierPath())
        }
        
        let size = NSSize(width: 440, height: 150)
        
        for (segmentIndex, segment) in self.envelope.segments.enumerated() {
            let path = areaPaths[segmentIndex]
            
            NSGraphicsContext.saveGraphicsState()
            
            path.move(to: NSPoint(x: 0, y: size.height / 2))  // move to left, middle height
            path.line(to: highPointAxis)
            path.line(to: highMax)
            path.line(to: initialMax)
            path.line(to: NSPoint(x: 0, y: size.height))
            path.close()
            
            backgroundColor.setFill()
            path.fill()
            
            NSGraphicsContext.restoreGraphicsState()
        }
    }
    
}


