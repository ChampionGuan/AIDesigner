#import "VibratorC.h"
#import "Vibrator.h"
#import <UIKit/UIKit.h>
#import <AVFoundation/AVFoundation.h>

@interface Vibrator ()<NSObject>
@end

static Vibrator * m_Instance = nil;
static UIImpactFeedbackGenerator * m_Generator = nil;
static enum UIImpactFeedbackStyle m_Style = UIImpactFeedbackStyleLight;
enum EVibratorIntensity{
    VIBRATOR_INTENSITY_LIGHT = 0,
    VIBRATOR_INTENSITY_MEDIUM = 1,
    VIBRATOR_INTENSITY_HEAVY = 2,
};
static float m_DelteTime = 0.0001;
static enum EVibratorIntensity m_VibratorIntensity = VIBRATOR_INTENSITY_LIGHT;
static int m_SystemSoundId = kSystemSoundID_Vibrate;
static bool m_SystemSoundComplete = false;
//单个
static NSTimer * m_Timer = nil;
static float m_TimeInternal = 0;
static float m_TimeInternalDelta = 0;
static long m_RepeatTotalCount = 0;
static long m_RepeatCount = 0;
static bool m_IsGroupFinish = false;
//Group
static NSTimer * m_GroupTimer = nil;
static float m_GroupTimeInternal = 0;
static float m_GroupTimeInternalDelta = 0;
static long m_GroupRepeatTotalCount = 0;
static long m_GroupRepeatCount = 0;


@implementation Vibrator;

+ (Vibrator *)Instance {
    @synchronized(self){
        if(m_Instance == nil){
            m_Instance = [[self alloc] init];
        }
    }
    return m_Instance;
}

+ (UIImpactFeedbackGenerator *) Generator{
    if(m_Generator == nil){
        m_Generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: m_Style];
    }
    return m_Generator;
}

- (void) Play: (int)vibratorIntensity:(float)timeInternal:(long) repeatCount: (float)groupTimeInternal: (long)groupRepeatCount{
    [[Vibrator Instance] Stop];

    m_VibratorIntensity = (EVibratorIntensity)vibratorIntensity;
    switch (m_VibratorIntensity) {
        case VIBRATOR_INTENSITY_MEDIUM:
            m_Style = UIImpactFeedbackStyleMedium;
            m_SystemSoundId = 1520;
            printf("[Vibrator]Medium\n");
            break;
        case VIBRATOR_INTENSITY_HEAVY:
            m_Style = UIImpactFeedbackStyleHeavy;
            m_SystemSoundId = kSystemSoundID_Vibrate;
            printf("[Vibrator]Heavy\n");
            break;
        default:
            m_Style = UIImpactFeedbackStyleLight;
            m_SystemSoundId = 1519;
            printf("[Vibrator]Light\n");
            break;
    }
    m_TimeInternal = timeInternal;
    m_RepeatTotalCount = repeatCount;
    m_GroupTimeInternal = groupTimeInternal;
    m_GroupRepeatTotalCount = groupRepeatCount;
    //默认会调用一次，所以这里是1
    m_RepeatCount = 1;
    m_GroupRepeatCount = 0;
    m_TimeInternalDelta = 0;
    m_GroupTimeInternalDelta = 0;
    m_SystemSoundComplete = false;
    m_IsGroupFinish = false;
    //开启Update
    m_Timer = [NSTimer scheduledTimerWithTimeInterval:m_DelteTime target:self selector:@selector(Update) userInfo:(nil) repeats:(BOOL)true];
    
    //注册回调
    AudioServicesAddSystemSoundCompletion(m_SystemSoundId, NULL, NULL,SystemSoundComplete, NULL);
    SystemSoundPlay();

}

- (void) Update{
    if(!m_SystemSoundComplete)
        return;
    
    if(m_IsGroupFinish)
    {
        m_GroupTimeInternalDelta += m_DelteTime;
        if(m_GroupTimeInternalDelta >= m_GroupTimeInternal)
        {
            m_GroupRepeatCount += 1;
            if(m_GroupRepeatTotalCount >= 0)
            {
                if(m_GroupRepeatCount > m_GroupRepeatTotalCount)
                {
                    [[Vibrator Instance] Stop];
                    return;
                }
            }
            //开启下一轮循环
            m_GroupTimeInternalDelta = 0;
            m_RepeatCount = 0;
            m_TimeInternalDelta = 0;
            m_IsGroupFinish = false;
            printf("[Vibrator]下一组震动开始\n");
        }
    }
    
    //到间隔了播放一次
    if(!m_IsGroupFinish)
    {
        m_TimeInternalDelta+=m_DelteTime;
        if(m_TimeInternalDelta>m_TimeInternal){
            m_RepeatCount+=1;
    
            if(m_RepeatTotalCount >= 0)
            {
                if(m_RepeatCount > m_RepeatTotalCount)
                {
                    m_IsGroupFinish = true;
                    printf("[Vibrator]当前组震动完成\n");
                    return;
                }
            }
            //printf("AudioServicesPlaySystemSound");
            m_TimeInternalDelta=0;
            m_SystemSoundComplete = false;
            SystemSoundPlay();

        }
    }
}

void SystemSoundPlay()
{
    AudioServicesPlaySystemSound(m_SystemSoundId);

    /*if(@available(iOS 10.0,*)){
        [[Vibrator Generator] initWithStyle:m_Style];
        [[Vibrator Generator] prepare];
        [[Vibrator Generator] impactOccurred];
        
    }*/
}

void SystemSoundComplete(SystemSoundID soundId,void * clientData)
{
    m_SystemSoundComplete = true;
    printf("[Vibrator]SystemSoundComplete\n");
}

- (void) Stop{
    if(m_Timer != nil)
    {
        [m_Timer invalidate];
    }
    AudioServicesRemoveSystemSoundCompletion(m_SystemSoundId);
    printf("[Vibrator]所有组震动停止\n");
}

@end

#if defined (__cplusplus)
extern "C"
{
#endif

void VibratorPlay(int vibratorIntensity,float time,long repeatCount,float groupTime,long groupRepeatCount){
    [[Vibrator Instance] Play: vibratorIntensity:time:repeatCount:groupTime:groupRepeatCount];
}

void VibratorStop(){
    [[Vibrator Instance] Stop];
}

#if defined (__cplusplus)
}
#endif
