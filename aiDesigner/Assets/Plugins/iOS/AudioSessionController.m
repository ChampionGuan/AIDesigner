#import "AudioSessionController.h"
@implementation AudioSessionController
+ (BOOL) Active : (BOOL)unityApi{
	BOOL result = YES;
	
	if(unityApi == TRUE)
	{
		UnitySetAudioSessionActive(TRUE);
	}
	else
	{
	    NSError *error;
		AVAudioSession *session = [AVAudioSession sharedInstance];
		[session setActive:YES error:&error];
		if (nil != error) {
			NSLog(@"AudioSession setActive error:%@", error.localizedDescription);
			result = NO;
		}
	}
	[[UIApplication sharedApplication] beginReceivingRemoteControlEvents];
	return result;
}

+ (BOOL)Open {
    NSLog(@"Open");
	return [AudioSessionController SetCategory:AVAudioSessionCategoryPlayback Options:AVAudioSessionCategoryOptionDuckOthers];
}

+ (BOOL)Close {
    NSLog(@"Close");
	return [AudioSessionController SetCategory:AVAudioSessionCategoryAmbient Options:AVAudioSessionCategoryOptionDuckOthers];
}

//+ (BOOL)SetCategory:(NSString *)category mode:(NSString *)mode Options:(AVAudioSessionCategoryOptions)options Error:(NSError **)outError {
//}

+ (BOOL)SetCategory:(NSString *)category  Options:(NSUInteger)options{
    NSLog(@"SetCategory");
	BOOL result = YES;
	NSError *error = nil;
	NSLog(@"Current Category:%@", [AVAudioSession sharedInstance].category);
	AVAudioSessionCategoryOptions _currentoptions = [[AVAudioSession sharedInstance] categoryOptions];
	NSLog(@"Category[%@] has %lu options",  [AVAudioSession sharedInstance].category, _currentoptions);
	[[AVAudioSession sharedInstance] setCategory:category withOptions:options error:&error];
	if (nil != error) {
		NSLog(@"set Option error %@", error.localizedDescription);
		result = NO;
	}
	_currentoptions = [[AVAudioSession sharedInstance] categoryOptions];
	NSLog(@"Category[%@] has %lu options",  [AVAudioSession sharedInstance].category, _currentoptions);
	return result;
}

@end

#if defined (__cplusplus)
extern "C" {
#endif

    void ActiveAudioSession(bool useUnityApi)
    {
        [AudioSessionController Active : useUnityApi];
    }
	
    void OpenAudioSession()
    {
        [AudioSessionController Open];
    }
	
	void CloseAudioSession()
    {
        [AudioSessionController Close];
    }
	
	void SetAudioSession(char* category, uint options)
    {
        NSString* categoryKey = [NSString stringWithUTF8String:category];
		NSUInteger optionsKey = (NSInteger)options;
        [AudioSessionController SetCategory:categoryKey Options:optionsKey];
    }
    
#if defined (__cplusplus)
}
#endif