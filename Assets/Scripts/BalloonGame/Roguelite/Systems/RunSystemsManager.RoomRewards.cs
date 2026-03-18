using UnityEngine;

public partial class RunSystemsManager
{
    public RoomRewardPreview PreviewRoomRewards(int currentScore, int targetScore)
    {
        int overflow = Mathf.Max(0, currentScore - targetScore);
        int tickets = Mathf.RoundToInt(overflow / Mathf.Max(1f, GameConfigSO.Instance.overflowScorePerTicket) * (1f + _perkManager.OverflowTicketRateBonus));
        float finisher = overflow / Mathf.Max(1f, GameConfigSO.Instance.overflowScorePerFinisherCharge) * (1f + _perkManager.OverflowFinisherRateBonus);
        return new RoomRewardPreview
        {
            OverflowScore = overflow,
            TicketGain = tickets,
            FinisherGain = finisher,
        };
    }

    public void FinalizeRoomRewards(int currentScore, int targetScore)
    {
        RoomRewardPreview preview = PreviewRoomRewards(currentScore, targetScore);
        GainTickets(preview.TicketGain);
        GainFinisherCharge(preview.FinisherGain);
    }
}
