// Copyright (C) DALOG Diagnosesysteme GmbH - All Rights Reserved

namespace Dalog.Foundation.BackgroundServices;

/// <summary>
/// Defines options for a background service, including a method to validate the configuration.
/// </summary>
public interface IBackgroundServiceOptions
{
    /// <summary>
    /// Validates the current configuration options.
    /// Throws an exception if the configuration is invalid.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if the configuration options are invalid.
    /// </exception>
    void Validate();
}
