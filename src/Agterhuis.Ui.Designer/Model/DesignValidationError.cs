namespace Agterhuis.Ui.Designer.Model;

public sealed record DesignValidationError(
	string Path,
	string Code,
	string Message,
	DesignValidationSeverity Severity = DesignValidationSeverity.Error,
	int? PageIndex = null,
	string? NodeId = null,
	string? ParameterName = null);