/**
 * School Certificate PDF — "VËRTETIM" (Auto Shkolla Linda)
 *
 * Landscape A4, matches the official certificate template.
 * Auto-fills candidate data; leaves theory/practice hours and signatures blank.
 */
import html2pdf from "html2pdf.js";

const esc = (v) =>
  String(v ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");

function formatDate(raw) {
  if (!raw) return "";
  if (/^\d{2}\.\d{2}\.\d{4}$/.test(raw)) return raw;
  const d = new Date(raw);
  if (isNaN(d.getTime())) return String(raw);
  return `${String(d.getDate()).padStart(2, "0")}.${String(d.getMonth() + 1).padStart(2, "0")}.${d.getFullYear()}`;
}

function pnBoxes(personalNumber, count = 10) {
  const digits = String(personalNumber ?? "").replace(/\D/g, "").slice(0, count).split("");
  let cells = "";
  for (let i = 0; i < count; i++) {
    cells += `<td class="pn-box">${esc(digits[i] ?? "")}</td>`;
  }
  return `<table class="pn-tbl" cellspacing="0" cellpadding="0"><tr>${cells}</tr></table>`;
}

function u(text, w = "120px") {
  return `<span class="u" style="min-width:${w}">${text ? esc(text) : "&nbsp;"}</span>`;
}

export function generateCertificateHtml(candidate) {
  const c = candidate || {};

  const fullName = [c.firstName, c.parentName ? `(${c.parentName})` : "", c.lastName]
    .filter(Boolean).join(" ");
  const dob = formatDate(c.dateOfBirth || "");
  const pob = c.placeOfBirth || "";
  const municipality = c.municipality || c.address || "";
  const residence = c.address || "";
  const personalNumber = c.personalNumber || "";
  const regDate = formatDate(c.dateAdded || "");
  const serialNumber = c.serialNumber || "";
  const category = c.categoryName || c.category || "";

  return `
  <div class="cert">
    <style>
      .cert {
        width: 297mm;
        height: 210mm;
        box-sizing: border-box;
        padding: 10mm 16mm 10mm 20mm;
        font-family: "Times New Roman", Times, serif;
        color: #000;
        background: #fff;
        font-size: 11pt;
        line-height: 1.5;
        position: relative;
        overflow: hidden;
      }

      /* Vertical side line */
      .cert::before {
        content: "";
        position: absolute;
        left: 12mm;
        top: 8mm;
        bottom: 8mm;
        width: 1.5px;
        background: #000;
      }

      /* Document number */
      .cert .doc-no {
        text-align: right;
        font-size: 10pt;
        margin-bottom: 2mm;
      }

      /* Header */
      .cert .hdr {
        text-align: center;
        margin-bottom: 1mm;
      }
      .cert .school-name {
        font-size: 28pt;
        font-weight: 700;
        letter-spacing: 2px;
        line-height: 1.1;
      }
      .cert .hdr-row {
        font-size: 10.5pt;
        line-height: 1.5;
      }
      .cert .hdr-sub {
        font-size: 8pt;
        font-style: italic;
        margin-top: -1mm;
      }
      .cert .lic-row {
        text-align: center;
        font-size: 10.5pt;
        margin-bottom: 4mm;
      }

      /* Title */
      .cert .title {
        text-align: center;
        font-size: 20pt;
        font-weight: 700;
        letter-spacing: 5px;
        margin: 4mm 0 5mm 0;
      }

      /* Underline spans */
      .cert .u {
        display: inline-block;
        border-bottom: 1px solid #000;
        min-height: 1em;
        padding: 0 4px;
        text-align: center;
        vertical-align: baseline;
      }

      /* Body */
      .cert .body {
        font-size: 10.5pt;
        line-height: 1.65;
        text-align: justify;
      }
      .cert .body .sub-label {
        font-size: 7.5pt;
        font-style: italic;
        display: block;
        margin-top: -1.5mm;
        margin-bottom: 0.5mm;
      }

      /* Personal number boxes */
      .cert .pn-tbl {
        display: inline-table;
        border-collapse: collapse;
        vertical-align: middle;
      }
      .cert .pn-box {
        width: 5mm;
        height: 5mm;
        border: 1px solid #000;
        text-align: center;
        font-weight: 700;
        font-size: 8.5pt;
        padding: 0;
      }

      /* Hours */
      .cert .hours {
        font-size: 10.5pt;
        line-height: 1.75;
        margin: 1mm 0;
      }

      /* Assessment */
      .cert .assess {
        font-size: 10.5pt;
        line-height: 1.65;
        margin-top: 2mm;
      }
      .cert .opinion {
        display: flex;
        align-items: baseline;
        gap: 3px;
        min-height: 7mm;
      }
      .cert .opinion-lbl {
        font-weight: 700;
        white-space: nowrap;
      }
      .cert .opinion-val {
        flex: 1;
        border-bottom: 1px solid #000;
        min-height: 5mm;
      }

      /* Signatures */
      .cert .sigs {
        display: flex;
        justify-content: space-between;
        margin-top: 6mm;
        font-size: 10.5pt;
      }
      .cert .sig-col { text-align: center; }
      .cert .sig-line {
        display: inline-block;
        border-bottom: 1px solid #000;
        width: 50mm;
        height: 7mm;
      }

      /* Footer */
      .cert .footer {
        display: flex;
        justify-content: space-between;
        align-items: flex-end;
        margin-top: 5mm;
        font-size: 10.5pt;
      }
      .cert .stamp { width: 28mm; height: 28mm; }
    </style>

    <div class="doc-no">N: ${u("", "50px")}</div>

    <!-- Header -->
    <div class="hdr">
      <div class="school-name">Linda</div>
      <div class="hdr-row">
        Auto Shkolla ${u("Linda", "60px")} me seli në ${u("PRISHTINË", "80px")} adresa ${u("BILL KLINTON", "100px")}
      </div>
      <div class="hdr-sub">(Emri i Auto Shkollës)</div>
    </div>

    <div class="lic-row">
      dhe numër të licencës ${u("01-09/13", "70px")} lëshon këtë
    </div>

    <!-- Title -->
    <div class="title">V Ë R T E T I M</div>

    <!-- Body -->
    <div class="body">
      ${u(fullName, "200px")}
      i-e lindur më ${u(dob, "80px")} në ${u(pob, "90px")} komuna ${u(municipality, "90px")}
      <span class="sub-label">(Emri, emri i njërit prind, mbiemri)</span>

      me vendbanim në ${u(residence, "90px")}
      &nbsp; nr. personal ${pnBoxes(personalNumber, 10)}
      i,e regjistruar në auto shkollë më
      datë ${u(regDate, "70px")} numër rendor ${u(serialNumber, "50px")},
      kreu aftësimin për dhënien e provimit për shofer për kategorinë
      ${u(category, "26px")} sipas plan
      programit të paraparë nga lëndët mësimore:
    </div>

    <!-- Hours (left empty) -->
    <div class="hours">
      <div>-Rregullat e komunikacionit dhe të sigurisë (pjesa teorike) prej ${u("", "26px")} orëve në kohën prej: ${u("", "70px")} deri: ${u("", "70px")}</div>
      <div>-Të drejtuarit e mjetit me veprim motorik (pjesa praktike) prej ${u("", "26px")} orëve në kohën prej: ${u("", "70px")} deri: ${u("", "70px")}</div>
    </div>

    <!-- Assessment (left empty for handwriting) -->
    <div class="assess">
      <div>Pas verifikimit të aftësive të kandidatit-es jepet mendimi:</div>
      <div class="opinion">
        <span class="opinion-lbl">-Ligjëruesit:</span>
        <span class="opinion-val"></span>
      </div>
      <div class="opinion">
        <span class="opinion-lbl">-Instruktorit:</span>
        <span class="opinion-val"></span>
      </div>
    </div>

    <!-- Signatures (left empty) -->
    <div class="sigs">
      <div class="sig-col">
        Nënshkrimi i ligjëruesit<br/>
        <div class="sig-line"></div>
      </div>
      <div class="sig-col">
        Nënshkrimi i instruktorit<br/>
        <div class="sig-line"></div>
      </div>
    </div>

    <!-- Footer -->
    <div class="footer">
      <div>Data e lëshuarjes së vërtetimit: ${u("", "80px")}</div>
      <div class="stamp"></div>
      <div>Drejtori: ${u("", "60px")}</div>
    </div>
  </div>
  `;
}

export async function downloadCertificatePdf(candidate) {
  const c = candidate || {};

  const container = document.createElement("div");
  container.style.position = "fixed";
  container.style.left = "-9999px";
  container.style.top = "0";
  container.innerHTML = generateCertificateHtml(c);
  document.body.appendChild(container);

  const element = container.firstElementChild;

  const lastName = (c.lastName || "Candidate").replace(/\s+/g, "_");
  const firstName = (c.firstName || "").replace(/\s+/g, "_");
  const filename = `Vertetim_Autoshkolla_${firstName}_${lastName}.pdf`;

  try {
    await html2pdf()
      .set({
        margin: 0,
        filename,
        image: { type: "jpeg", quality: 0.98 },
        html2canvas: {
          scale: 2,
          useCORS: true,
          letterRendering: true,
        },
        jsPDF: { unit: "mm", format: "a4", orientation: "landscape" },
        pagebreak: { mode: ["avoid-all"] },
      })
      .from(element)
      .save();
  } finally {
    document.body.removeChild(container);
  }
}
