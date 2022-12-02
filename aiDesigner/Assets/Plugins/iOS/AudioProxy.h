#pragma once
#import "MediaPlayer/MediaPlayer.h"
#import "AVFoundation/AVFoundation.h"

typedef int (*ActionHandler) (int actionType);
typedef enum ActionType : NSUInteger {
	ActionTypeActive = 0,	
	ActionTypeInit = 1,
    ActionTypePlay = 2,
    ActionTypePause = 3,
    ActionTypeStop = 4,
    ActionTypeNext = 5,
    ActionTypePrev = 6,
    ActionTypeGetDatas = 7,
    ActionTypeUpdate = 8,
    ActionTypePlayNext = 9,
    ActionTypePlayPrev = 10,
} ActionType;

NSString * const ActionDataKeyName = @"Name";
NSString * const ActionDataKeyTitle = @"Title";
NSString * const ActionDataKeyImage = @"Image";
NSString * const ActionDataKeyPrevState = @"PrevState";
NSString * const ActionDataKeyNextState = @"NextState";
NSString * const ActionDataKeyPlayState = @"PlayState";

NSString * const ActionStatePlay = @"Play";
NSString * const ActionStatePause = @"Pause";

NSString * const ActionStateOpen = @"Open";
NSString * const ActionStateClose = @"Close";
//curTime - totalTime
NSString * const ActionStateCurTime = @"CurTime";
NSString * const ActionStateTotalTime = @"TotalTime";


@interface AudioProxy: NSObject
{
	@public
	float updatePeroid;
	NSTimer* backgroundDelayTimer;
	NSTimer* backgroundUpdateTimer;
    AVAudioPlayer* _audioPlayer;
    //AVQueuePlayer* _audioPlayer;
	@public
    AVAudioSessionCategory category;
	@public
    AVAudioSessionCategoryOptions options;
    @public
    BOOL isBackgroundMode;
}

@property(nonatomic, strong) AVAudioPlayer* audioPlayer;

/*单例*/
+ (instancetype)defaultInstance;
+ (instancetype)new NS_UNAVAILABLE;
- (id)copy NS_UNAVAILABLE;
- (id)mutableCopy NS_UNAVAILABLE;

//初始化参数
- (void)ctor;
+ (void)destroyInstance;

- (void)updateBackgroundMPRemote;
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlay;
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePause;
- (MPRemoteCommandHandlerStatus)backgroundMPRemoteNext;
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePrev;
- (MPRemoteCommandHandlerStatus)backgroundMPRemoteStop;
- (MPRemoteCommandHandlerStatus)backgroundMPRemoteTogglePlayPause;
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlayNext;
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlayPrev;

//自定义获取数据更新MPRemote
- (MPRemoteCommandHandlerStatus)backgroundMPRemoteFetchDatas;
- (BOOL)executeAction:(ActionType)actionType;
- (void)startBackgroundMode:(BOOL)activeState;
- (void)awakeBackgroundMode:(BOOL)activeState;
- (void)startUpdateTimer:(float)period;
- (void)stopUpdateTimer;
- (void)setDelayTimerActive:(BOOL)activeState;
- (void)tryInvokeBackgroundMode;

//播放后台AVAudioPlayer
-(void)togglePlayOrPauseAVAudioPlayer:(BOOL)activeState;
-(void)stopAVAudioPlayer;
-(void)startAVAudioPlayer;
@end
