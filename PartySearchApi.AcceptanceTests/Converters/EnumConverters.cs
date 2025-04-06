using PartySearchApi.Api.Models;
using Reqnroll;

namespace PartySearchApi.AcceptanceTests.Converters
{
    [Binding]
    public class EnumConverters
    {
        [StepArgumentTransformation]
        public PartyType ConvertToPartyType(string typeString)
        {
            return Enum.Parse<PartyType>(typeString);
        }

        [StepArgumentTransformation]
        public SanctionsStatus ConvertToSanctionsStatus(string statusString)
        {
            return statusString switch
            {
                "Pending Review" => SanctionsStatus.PendingReview,
                "Confirmed Match" => SanctionsStatus.ConfirmedMatch,
                "False Positive" => SanctionsStatus.FalsePositive,
                _ => Enum.Parse<SanctionsStatus>(statusString)
            };
        }
    }
}