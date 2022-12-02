#import "AudioProxy.h"

//C#端回调
static ActionHandler _CallActionHandler;
static NSMutableDictionary* wwiseDataDict;

@implementation AudioProxy

@synthesize audioPlayer = _audioPlayer;
-(void)startBackgroundMode:(BOOL)activeState
{
    if(activeState == YES && isBackgroundMode == YES)
    {
        [self setDelayTimerActive:NO];
        [self executeAction:ActionTypeInit];
        [self backgroundMPRemoteFetchDatas];
    }
    else
    {
        //[self stopUpdateTimer];
        [self setDelayTimerActive:NO];
    }
}

-(void)updateBackgroundMPRemote
{
    if(isBackgroundMode == YES && wwiseDataDict != NULL && wwiseDataDict.count > 0)
    {
        BOOL _isPlayingInfoDirty = NO;
        MPRemoteCommandCenter* commandcenter = [MPRemoteCommandCenter sharedCommandCenter];
        MPNowPlayingInfoCenter* center = [MPNowPlayingInfoCenter defaultCenter];
        NSMutableDictionary* playingInfo = NULL;
        if(center.nowPlayingInfo != NULL)
        {
            printf("->center.nowPlayingInfo != NULL\n");
            playingInfo = [center.nowPlayingInfo mutableCopy];
        }
        
        if(playingInfo == NULL)
        {
            printf("->center.nowPlayingInfo == NULL\n");
            playingInfo = [[NSMutableDictionary alloc] init];
            playingInfo[MPNowPlayingInfoPropertyPlaybackRate] = [NSNumber numberWithFloat:1.0];
            playingInfo[MPNowPlayingInfoPropertyIsLiveStream] = false;
            playingInfo[MPNowPlayingInfoPropertyMediaType] = [NSNumber numberWithInt:MPNowPlayingInfoMediaTypeAudio];
        }
        
        for(NSString* key in wwiseDataDict)
        {
            NSString *value = wwiseDataDict[key];
            if([key isEqualToString:ActionDataKeyName] == YES)
            {
                playingInfo[MPMediaItemPropertyTitle] = value;
                _isPlayingInfoDirty = YES;
            }
            else if([key isEqualToString:ActionDataKeyTitle] == YES)
            {
                playingInfo[MPMediaItemPropertyArtist] = value;
                _isPlayingInfoDirty = YES;
            }
            else if([key isEqualToString:ActionDataKeyImage] == YES)
            {
                UIImage* image = [UIImage imageWithContentsOfFile:value];
                MPMediaItemArtwork* item  = NULL;

                if(@available(iOS 10.0, *))
                {
                    item  = [[MPMediaItemArtwork alloc] initWithBoundsSize:image.size requestHandler:^UIImage * _Nonnull(CGSize size) {
                        return image;
                    }];
                }
                else
                {
                    item  = [[MPMediaItemArtwork alloc] initWithImage:image];
                }
                playingInfo[MPMediaItemPropertyArtwork] = item;
                _isPlayingInfoDirty = YES;
            }
            else if([key isEqualToString:ActionDataKeyPrevState] == YES)
            {
                 //commandcenter.previousTrackCommand.enabled = [value isEqualToString:ActionStateOpen] == YES;
            }
            else if([key isEqualToString:ActionDataKeyNextState] == YES)
            {
                 //commandcenter.nextTrackCommand.enabled = [value isEqualToString:ActionStateOpen] == YES;
            }
            else if([key isEqualToString:ActionDataKeyPlayState] == YES)
            {
                if([value isEqualToString:ActionStatePlay] == YES)
                {
                    playingInfo[MPNowPlayingInfoPropertyPlaybackRate] = [NSNumber numberWithFloat:1.0];
                    playingInfo[MPNowPlayingInfoPropertyMediaType] = [NSNumber numberWithInt:MPNowPlayingInfoMediaTypeAudio];
                    _isPlayingInfoDirty = YES;
					
                    [self togglePlayOrPauseAVAudioPlayer:YES];//wwise2019
					[self backgroundMPRemotePlay];
					[[AVAudioSession sharedInstance] setActive:YES error:nil];
                }
                else
                {
				    [self backgroundMPRemotePause];
                    [self togglePlayOrPauseAVAudioPlayer:NO];//wwise2019
					[[AVAudioSession sharedInstance] setActive:NO error:nil];

                    playingInfo[MPNowPlayingInfoPropertyPlaybackRate] = [NSNumber numberWithFloat:0.0];
                    playingInfo[MPNowPlayingInfoPropertyMediaType] = [NSNumber numberWithInt:MPNowPlayingInfoMediaTypeNone];
                    _isPlayingInfoDirty = YES;
                }
            }
            else if([key isEqualToString:ActionStateCurTime] == YES)
            {
                            printf("->center.ActionStateCurTime != NULL\n ");
                playingInfo[MPNowPlayingInfoPropertyElapsedPlaybackTime] = [NSNumber numberWithFloat:[value integerValue] / 1000];

            }
            else if([key isEqualToString:ActionStateTotalTime] == YES)
            {
                playingInfo[MPMediaItemPropertyPlaybackDuration] = [NSNumber numberWithFloat:[value integerValue] / 1000];
            }
        }
        
        //修改playingInfo
        if(_isPlayingInfoDirty == YES)
        {
            center.nowPlayingInfo = playingInfo;
        }
    }
}

-(void)togglePlayOrPauseAVAudioPlayer:(BOOL)activeState
{
   if(self.audioPlayer != NULL)
   {
      MPNowPlayingInfoCenter* center = [MPNowPlayingInfoCenter defaultCenter];
      if(activeState == YES)
      {
        [self.audioPlayer play];
        if(@available(iOS 13.0, *))
            center.playbackState = MPNowPlayingPlaybackStatePlaying;
      }
      else
      {
        [self.audioPlayer pause];
        if(@available(iOS 13.0, *))
            center.playbackState = MPNowPlayingPlaybackStatePaused;
      }
   }
}

-(void)stopAVAudioPlayer
{
   if(self.audioPlayer != NULL)
   {
      [self.audioPlayer stop];
      self.audioPlayer = NULL;
      MPNowPlayingInfoCenter* center = [MPNowPlayingInfoCenter defaultCenter];
      if(@available(iOS 13.0, *))
        center.playbackState = MPNowPlayingPlaybackStateStopped;
   }
}

-(void)startAVAudioPlayer
{
    //play default music
    NSURL* url = [[NSBundle mainBundle] URLForResource:@"Data/Raw/Audio" withExtension:@"wav"];
    NSLog(@"->url:%@", url.absoluteString);
    [self stopAVAudioPlayer];
    
    self.audioPlayer = [[AVAudioPlayer alloc] initWithContentsOfURL:url error:nil];
    self.audioPlayer.currentTime = 0;
    self.audioPlayer.volume = 0;
    self.audioPlayer.numberOfLoops = -1;
    [self.audioPlayer prepareToPlay];
    [self.audioPlayer play];

   /* NSMutableArray *tempArray = [NSMutableArray array];
   // AVPlayerItem *itme = [AVPlayerItem playerItemWithURL:[NSURL URLWithString:musicUrl]];
    NSURL* url2 = [[NSBundle mainBundle] URLForResource:@"Data/Raw/Audio2" withExtension:@"wav"];
    AVPlayerItem *item = [AVPlayerItem playerItemWithURL:url];
    [tempArray addObject:item];
    AVPlayerItem *item2 = [AVPlayerItem playerItemWithURL:url2];
    [tempArray addObject:itme];
    self.audioPlayer = [[AVQueuePlayer alloc] initWithItems:tempArray];
    [self.audioPlayer play];
    */
}

-(void)awakeBackgroundMode:(BOOL)activeState
{
    isBackgroundMode = activeState;
    if(activeState == YES)
    {
        printf("->awakeBackgroundMode YES\n");
        //保存原始AudioSession设置
        category = [AVAudioSession sharedInstance].category;
        options = [AVAudioSession sharedInstance].categoryOptions;
        
        //允许后台播放
        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback
         withOptions:0 error:nil]; //AVAudioSessionCategoryOptionDefaultToSpeaker + 空数组[]
        //UnitySetAudioSessionActive(YES);
        [[AVAudioSession sharedInstance] setActive:YES error:nil];
        
        //接收事件
        [[UIApplication sharedApplication] beginReceivingRemoteControlEvents];
        
        //    MPMusicPlayerController* player = [MPMusicPlayerController applicationMusicPlayer];
        //    MPMediaItem* item = player.nowPlayingItem;
        //    AVPlayerItem* audioItem = [AVPlayerItem playerItemWithURL:item.assetURL];
        //    AVPlayer* audioPlayer= [AVPlayer playerWithPlayerItem:audioItem];
        MPRemoteCommandCenter* commandcenter = [MPRemoteCommandCenter sharedCommandCenter];
        commandcenter.playCommand.enabled = true;
        commandcenter.pauseCommand.enabled = true;
        commandcenter.stopCommand.enabled = true;
        commandcenter.togglePlayPauseCommand.enabled = true;
        commandcenter.nextTrackCommand.enabled = true;
        commandcenter.previousTrackCommand.enabled = true;
        
        [commandcenter.playCommand addTarget:self action:@selector(backgroundMPRemotePlay)];
        [commandcenter.pauseCommand addTarget:self action:@selector(backgroundMPRemotePause)];
        [commandcenter.togglePlayPauseCommand addTarget:self action:@selector(backgroundMPRemoteTogglePlayPause)];
        [commandcenter.nextTrackCommand addTarget:self action:@selector(backgroundMPRemotePlayNext)];
        [commandcenter.previousTrackCommand addTarget:self action:@selector(backgroundMPRemotePlayPrev)];
        
        NSMutableDictionary* playingInfo = [[NSMutableDictionary alloc] init];
        playingInfo[MPNowPlayingInfoPropertyPlaybackRate] = [NSNumber numberWithFloat:1.0];
        playingInfo[MPNowPlayingInfoPropertyIsLiveStream] = false;
        playingInfo[MPNowPlayingInfoPropertyMediaType] = [NSNumber numberWithInt:MPNowPlayingInfoMediaTypeAudio];
        MPNowPlayingInfoCenter* center = [MPNowPlayingInfoCenter defaultCenter];
        center.nowPlayingInfo = playingInfo;
        if(@available(iOS 13.0, *))
            center.playbackState = MPNowPlayingPlaybackStatePlaying;
        
        //[self startAVAudioPlayer];
    }
    else
    {
        printf("->awakeBackgroundMode NO\n");
        [self stopAVAudioPlayer];
        MPRemoteCommandCenter* commandcenter = [MPRemoteCommandCenter sharedCommandCenter];
        commandcenter.playCommand.enabled = false;
        commandcenter.pauseCommand.enabled = false;
        commandcenter.stopCommand.enabled = false;
        commandcenter.togglePlayPauseCommand.enabled = false;
        commandcenter.nextTrackCommand.enabled = false;
        commandcenter.previousTrackCommand.enabled = false;
        [commandcenter.playCommand removeTarget:self];
        [commandcenter.pauseCommand removeTarget:self];
        [commandcenter.togglePlayPauseCommand removeTarget:self];
        [commandcenter.nextTrackCommand removeTarget:self];
        [commandcenter.previousTrackCommand removeTarget:self];
    
        NSMutableDictionary* playingInfo = [[NSMutableDictionary alloc] init];
        MPNowPlayingInfoCenter* center = [MPNowPlayingInfoCenter defaultCenter];
        center.nowPlayingInfo = playingInfo;
        
       [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategorySoloAmbient
        withOptions:AVAudioSessionCategoryOptionDuckOthers error:nil]; //AVAudioSessionCategoryOptionDefaultToSpeaker
	   [[AVAudioSession sharedInstance] setActive:YES error:nil];
		//UnitySetAudioSessionActive(TRUE);
       //终止
       [[UIApplication sharedApplication] endReceivingRemoteControlEvents];
    }
}

-(void)stopUpdateTimer
{
    if(backgroundUpdateTimer != NULL)
    {
       [backgroundUpdateTimer invalidate];
        backgroundUpdateTimer = NULL;
    }
}

-(void)startUpdateTimer:(float)period
{
    [self stopUpdateTimer];
    if(period <= 0) return;
    
    backgroundUpdateTimer = [NSTimer scheduledTimerWithTimeInterval:period repeats:YES block:^(NSTimer * _Nonnull timer) {
        [self executeAction:ActionTypeUpdate];
    }];
}

-(void)setDelayTimerActive:(BOOL)activeState
{
    if(backgroundDelayTimer != NULL)
    {
       [backgroundDelayTimer invalidate];
        backgroundDelayTimer = NULL;
    }
    
    if(activeState == YES)
    {
       backgroundDelayTimer = [NSTimer scheduledTimerWithTimeInterval:0.005 repeats:NO block:^(NSTimer * _Nonnull timer) {
        //[self executeAction:ActionTypeActive];
        [self executeAction:ActionTypeInit];
        [self backgroundMPRemoteFetchDatas];
         [timer invalidate];
         backgroundDelayTimer = NULL;
       }];
    }
}

- (void)tryInvokeBackgroundMode
{
    if(isBackgroundMode == YES)
    {
        [self setDelayTimerActive:YES];
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlay
{
    printf("->backgroundMPRemotePlay\n");
    [self executeAction:ActionTypePlay];
	[[AVAudioSession sharedInstance] setActive:YES error:nil];
    return MPRemoteCommandHandlerStatusSuccess;
}

- (MPRemoteCommandHandlerStatus)backgroundMPRemotePause
{
    printf("->backgroundMPRemotePause\n");
    [self executeAction:ActionTypePause];
	[[AVAudioSession sharedInstance] setActive:NO error:nil];
    return MPRemoteCommandHandlerStatusSuccess;
}


- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlayNext
{
    printf("->backgroundMPRemotePlayNext\n");
    [self executeAction:ActionTypePlayNext];
    [self backgroundMPRemoteFetchDatas];
    return MPRemoteCommandHandlerStatusSuccess;
}

- (MPRemoteCommandHandlerStatus)backgroundMPRemotePlayPrev
{
    printf("->backgroundMPRemotePlayPrev\n");
    [self executeAction:ActionTypePlayPrev];
    [self backgroundMPRemoteFetchDatas];
    return MPRemoteCommandHandlerStatusSuccess;
}

- (MPRemoteCommandHandlerStatus)backgroundMPRemoteNext
{
    printf("->backgroundMPRemoteNext\n");
    [self executeAction:ActionTypeNext];
    [self backgroundMPRemoteFetchDatas];
    return MPRemoteCommandHandlerStatusSuccess;
}

- (MPRemoteCommandHandlerStatus)backgroundMPRemotePrev
{
    printf("->backgroundMPRemotePrev\n");
    [self executeAction:ActionTypePrev];
    [self backgroundMPRemoteFetchDatas];
    return MPRemoteCommandHandlerStatusSuccess;
}


- (MPRemoteCommandHandlerStatus)backgroundMPRemoteStop
{
    printf("->backgroundMPRemoteStop\n");
    [self executeAction:ActionTypeStop];
    return MPRemoteCommandHandlerStatusSuccess;
}

-(MPRemoteCommandHandlerStatus)backgroundMPRemoteTogglePlayPause{
    printf("->backgroundMPRemoteTogglePlayPause\n");
    if(self.audioPlayer && self.audioPlayer.isPlaying)
    {
        [self executeAction:ActionTypePause];
    }
    else
    {
        [self executeAction:ActionTypePlay];
    }
    return MPRemoteCommandHandlerStatusSuccess;
}

-(MPRemoteCommandHandlerStatus)backgroundMPRemoteFetchDatas
{
    printf("->backgroundMPRemoteFetchDatas\n");
    [self executeAction:ActionTypeGetDatas];
    [self updateBackgroundMPRemote];
    return MPRemoteCommandHandlerStatusSuccess;
}

-(BOOL)executeAction:(ActionType)actionType
{
    if(_CallActionHandler != NULL)
    {
        return _CallActionHandler((int)actionType);
    }
    return NO;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//全局静态实例AudioProxy
static AudioProxy *_instance = nil;
static dispatch_once_t onceToken;
    
+ (instancetype)defaultInstance {
    return [[self.class alloc] init];
}

+ (instancetype)allocWithZone:(struct _NSZone *)zone {
    
    if (_instance == nil) {
        dispatch_once(&onceToken, ^{
            _instance = [super allocWithZone:zone];
        });
    }
    return _instance;
}

- (instancetype)init {
    dispatch_once(&onceToken, ^{
        _instance = [super init];
        [_instance ctor];
    });
    return _instance;
}

- (void)ctor
{
    updatePeroid = 0;
    isBackgroundMode = NO;
}

+ (void)destroyInstance
{
    printf("->AudioProxy::destroyInstance\n");
    onceToken = 0;
    
    if(_instance != NULL)
    {
        [_instance stopUpdateTimer];
        [_instance setDelayTimerActive:NO];
        //销毁正在播放的声音
        [_instance stopAVAudioPlayer];
    }
    _instance = nil;
}
@end

#if defined (__cplusplus)
extern "C" {
#endif
    void Close()
    {
         [[AudioProxy defaultInstance] awakeBackgroundMode:NO];
        _CallActionHandler = NULL;
    }

    void Open(ActionHandler actionHandler)
    {
        _CallActionHandler = actionHandler;
        [[AudioProxy defaultInstance] awakeBackgroundMode:YES];
    }
    
    void OpenTimer(float peroid)
    {
        [AudioProxy defaultInstance]->updatePeroid = peroid;//原本想ios底层切换到后台生效
        [[AudioProxy defaultInstance] startUpdateTimer:peroid];    //算了还是直接触发吧
    }
    
    void CloseTimer()
    {
        [[AudioProxy defaultInstance] stopUpdateTimer];
        [AudioProxy defaultInstance]->updatePeroid = 0;
    }

    void SetDatas(int length, const char** wwiseDatas)
    {
        NSMutableDictionary *nsDic = [[NSMutableDictionary alloc] init];
        int kvCount = length / 2;
        NSLog(@"==SetDatas, length:==%d==kvCount:%d==", length, kvCount);
        for(int i = 0; i < kvCount; i++)
        {
            NSString *key = [NSString stringWithUTF8String:wwiseDatas[i * 2]];
            NSString *value = [NSString stringWithUTF8String:wwiseDatas[i * 2 + 1]];
            NSLog(@"===Key:%@, ===Value:%@", key, value);
            //[nsDic setValue:value forKey:key];
            nsDic[key] = value;
        }
        wwiseDataDict = [nsDic mutableCopy];
    }
    
    void UpdateRemote()
    {
         [[AudioProxy defaultInstance] backgroundMPRemoteFetchDatas];
    }
    
#if defined (__cplusplus)
}
#endif
