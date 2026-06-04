using System.Collections.Generic;
using System.Linq;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.Sequence;

namespace Chatbot.Shared.Brokers.Pii;

public class PiiBroker : IPiiBroker
{
    private const string DefaultCulture = Microsoft.Recognizers.Text.Culture.English;

    public string MaskSensitiveData(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        string result = text;

        // Mask Sequences (Emails, Phone numbers, URLs, IPs)
        result = MaskSequences(result);

        return result;
    }

    private static string MaskSequences(string text)
    {
        var results = SequenceRecognizer
            .RecognizeEmail(text, DefaultCulture)
            .Concat(SequenceRecognizer.RecognizePhoneNumber(text, DefaultCulture))
            .Concat(SequenceRecognizer.RecognizeIpAddress(text, DefaultCulture))
            .OrderByDescending(r => r.Start);

        string maskedText = text;

        foreach (var result in results)
        {
            string label = result.TypeName.ToUpperInvariant() switch
            {
                "EMAIL" => "[EMAIL]",
                "PHONENUMBER" => "[PHONE_NUMBER]",
                "IP" => "[IP_ADDRESS]",
                _ => $"[{result.TypeName.ToUpperInvariant()}]",
            };

            maskedText = maskedText
                .Remove(result.Start, result.End - result.Start + 1)
                .Insert(result.Start, label);
        }

        return maskedText;
    }
}
