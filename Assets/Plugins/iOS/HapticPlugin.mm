#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator *lightGenerator = nil;
static UIImpactFeedbackGenerator *mediumGenerator = nil;
static UIImpactFeedbackGenerator *heavyGenerator = nil;
static UISelectionFeedbackGenerator *selectionGenerator = nil;
static UINotificationFeedbackGenerator *notificationGenerator = nil;

static void EnsureGenerators() {
    if (lightGenerator == nil) {
        lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        [lightGenerator prepare];
    }
    if (mediumGenerator == nil) {
        mediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        [mediumGenerator prepare];
    }
    if (heavyGenerator == nil) {
        heavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
        [heavyGenerator prepare];
    }
    if (selectionGenerator == nil) {
        selectionGenerator = [[UISelectionFeedbackGenerator alloc] init];
        [selectionGenerator prepare];
    }
    if (notificationGenerator == nil) {
        notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
        [notificationGenerator prepare];
    }
}

extern "C" {
    void _HapticImpactLight() {
        EnsureGenerators();
        [lightGenerator impactOccurred];
        [lightGenerator prepare];
    }

    void _HapticImpactMedium() {
        EnsureGenerators();
        [mediumGenerator impactOccurred];
        [mediumGenerator prepare];
    }

    void _HapticImpactHeavy() {
        EnsureGenerators();
        [heavyGenerator impactOccurred];
        [heavyGenerator prepare];
    }

    void _HapticSelection() {
        EnsureGenerators();
        [selectionGenerator selectionChanged];
        [selectionGenerator prepare];
    }

    void _HapticNotificationSuccess() {
        EnsureGenerators();
        [notificationGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
        [notificationGenerator prepare];
    }

    void _HapticNotificationWarning() {
        EnsureGenerators();
        [notificationGenerator notificationOccurred:UINotificationFeedbackTypeWarning];
        [notificationGenerator prepare];
    }

    void _HapticNotificationError() {
        EnsureGenerators();
        [notificationGenerator notificationOccurred:UINotificationFeedbackTypeError];
        [notificationGenerator prepare];
    }
}
