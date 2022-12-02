#ifndef Vibrator_h
#define Vibrator_h

#import <AudioToolbox/AudioToolbox.h>
#import <Foundation/Foundation.h>

@interface Vibrator : NSObject
+ (Vibrator *)Instance;
+ (UIImpactFeedbackGenerator *) m_Generator;

- (void) Play:(int)vibratorIntensity:(float)timeInternal:(long)repeatCount: (float) groupTimeInternal: (long)groupRepeatCount;
- (void) Stop;
- (void) Update;
@end

#endif
