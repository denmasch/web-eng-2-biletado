using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Error;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Code
{
    [EnumMember(Value = "bad_request")]
    BadRequest,

    [EnumMember(Value = "not_authorized")]
    NotAuthorized,

    [EnumMember(Value = "no_need_to_know")]
    NoNeedToKnow
}