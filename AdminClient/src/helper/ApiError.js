// Centralised helpers for turning an API error / response into a clear,
// user-friendly message. Messages are in Albanian to match the UI language.

// Default messages keyed by HTTP status.
const STATUS_MESSAGES = {
  0: "Lidhja me serverin dështoi. Kontrolloni internetin dhe provoni përsëri.",
  400: "Të dhënat e dërguara nuk janë të vlefshme. Kontrolloni formularin dhe provoni përsëri.",
  401: "Sesioni juaj ka skaduar. Ju lutemi kyçuni përsëri.",
  403: "Nuk keni leje për të kryer këtë veprim.",
  404: "Regjistrimi nuk u gjet ose është fshirë.",
  409: "Ekziston tashmë një regjistrim me këto të dhëna (konflikt).",
  500: "Ndodhi një gabim në server gjatë ruajtjes së të dhënave. Provoni përsëri më vonë.",
};

// Pull the most meaningful message out of a response body, regardless of
// casing (the backend mixes Status/ResponseMsg and lowercase variants) or
// shape (Confirmation object vs. ASP.NET ModelState validation problem).
export function extractBodyMessage(body) {
  if (!body || typeof body !== "object") {
    return typeof body === "string" && body.trim() ? body.trim() : "";
  }

  // Confirmation-style: { Status, ResponseMsg }
  const direct =
    body.responseMsg ??
    body.ResponseMsg ??
    body.message ??
    body.Message ??
    body.error ??
    body.Error ??
    body.detail ??
    body.title;
  if (direct && typeof direct === "string" && direct.trim()) return direct.trim();

  // ASP.NET ModelState: { errors: { Field: ["msg", ...] } }
  const errors = body.errors ?? body.Errors;
  if (errors && typeof errors === "object") {
    const first = Object.values(errors)
      .flat()
      .find((m) => typeof m === "string" && m.trim());
    if (first) return first.trim();
  }

  return "";
}

// Returns true when the body represents a handled business error that uses the
// app's 202-with-Status="error" convention.
export function isBusinessError(body) {
  if (!body || typeof body !== "object") return false;
  const status = body.status ?? body.Status;
  return typeof status === "string" && status.toLowerCase() === "error";
}

// Build a clear message from an axios error, preferring the server's own text.
export function extractApiMessage(err, fallback) {
  const status = err?.response?.status ?? err?.request?.status ?? 0;
  const bodyMsg = extractBodyMessage(err?.response?.data);
  if (bodyMsg) return bodyMsg;
  if (STATUS_MESSAGES[status]) return STATUS_MESSAGES[status];
  return fallback || "Ndodhi një gabim i papritur. Provoni përsëri.";
}

export default { extractApiMessage, extractBodyMessage, isBusinessError };
