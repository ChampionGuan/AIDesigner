#import <AVFoundation/AVFoundation.h>
@interface AudioSessionController: NSObject
+ (BOOL) Active:(BOOL)unityApi;
+ (BOOL) Open;
+ (BOOL) Close;
+ (BOOL)SetCategory:(NSString *)category  Options:(NSUInteger)options;
@end
