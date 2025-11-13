using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace biletado_reservations_v3.Models.Error;

/// <summary>
/// An error response for an operation.
/// </summary>
public class ErrorContainer
{
    /// <summary>
    /// The array of error entries associated with the error response
    /// </summary>
    [JsonPropertyName("errors")]
    [MaxLength(100)]
    public List<Error> Errors { get; set; } = new();

    /// <summary>
    /// The error trace information.
    /// </summary>
    [JsonPropertyName("trace")]
    public Guid Trace { get; set; }
}