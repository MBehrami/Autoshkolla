/**
 * Kosovo Driving Exam Reservation Form – PDF-like HTML → PDF (html2pdf.js)
 *
 * ✅ Pika 8 jashtë kategorive
 * ✅ Kategoria e zgjedhur RRETHOHET (rreth rreth kutisë)
 * ✅ Remarks: vetëm logo e kompanisë (rowspan) – pa tekst “Linda”
 * ✅ Pika 10/11 bosh (nuk plotësohet asgjë)
 * ✅ “Nr.Regj / book.no.” shfaq vlerën (sipër + te slip poshtë)
 * ✅ Fundi (slip) pozicionohet me flex/margin-top:auto që të mos pritet
 */
import html2pdf from "html2pdf.js";

/** Helpers */
const esc = (v) =>
  String(v ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");

function buildPersonalNumberBoxes(personalNumber, boxCount = 10) {
  const digits = String(personalNumber ?? "")
    .replace(/\D/g, "")
    .slice(0, boxCount)
    .split("");

  const cells = [];
  for (let i = 0; i < boxCount; i++) {
    cells.push(`<td class="pn-box">${esc(digits[i] ?? "")}</td>`);
  }

  return `
    <table class="pn-table" cellspacing="0" cellpadding="0">
      <tr>${cells.join("")}</tr>
    </table>
  `;
}

/**
 * Categories strip like PDF:
 * - "8." is OUTSIDE categories (left)
 * - Selected category is CIRCLED around the category square
 * - Only category boxes (no extra labels under)
 */
function buildCategoriesStrip(selectedCategory) {
  const cats = [
    "AM",
    "A1",
    "A2",
    "A",
    "B1",
    "B",
    "C1",
    "C",
    "D1",
    "D",
    "BE",
    "C1E",
    "CE",
    "D1E",
    "DE",
    "M",
    "L",
    "T",
  ];
  const sel = String(selectedCategory ?? "").trim().toUpperCase();

  const cells = cats
    .map((c) => {
      const checked = sel === c.toUpperCase();
      return `
        <td class="cat-td">
          <div class="cat-box ${checked ? "selected" : ""}">${esc(c)}</div>
        </td>
      `;
    })
    .join("");

  return `
    <div class="cat-row">
      <div class="cat-point">8.</div>
      <table class="cat-table" cellspacing="0" cellpadding="0">
        <tr>${cells}</tr>
      </table>
    </div>
  `;
}

/**
 * Documents rows with Remarks big cell (logo only)
 */
function buildDocumentsRows(companyLogoDataUrl) {
  const docs = [
    "Vërtetimi i Auto Shkollës / Potvrda Auto Škole / Driving school certificate",
    "Certifikata shëndetësore / Zdravstveno uverenje / Health certificate",
    "Certifikata e kryqit të kuq / Uverenje crvenog krsta / Red Cross certificate",
    "Fotokopja e letërnjoftimit të R. Kosovës / Fotokopija lične karte R. Kosova / Photocopy of ID Cards of R. Kosovo",
  ];

  const logoHtml = companyLogoDataUrl
    ? `<img class="company-logo" src="${esc(companyLogoDataUrl)}" alt="Company logo" />`
    : `<div class="company-logo-placeholder"></div>`;

  return docs
    .map((d, idx) => {
      const remarksCell =
        idx === 0
          ? `<td class="docs-col-rem logo-cell" rowspan="4">${logoHtml}</td>`
          : "";

      return `
        <tr>
          <td class="docs-col-doc">${esc(d)}</td>
          <td class="docs-col-yes">PO</td>
          ${remarksCell}
        </tr>
      `;
    })
    .join("");
}

/** Signature box like PDF (gray header, empty body) */
function signatureBoxHTML(pointNo, titleLine) {
  return `
    <div class="sig-box">
      <div class="sig-head">
        <span class="sig-no">${pointNo}.</span>
        <span class="sig-text">${titleLine}</span>
      </div>
      <div class="sig-body"></div>
    </div>
  `;
}

/** Bottom slip (keeps 10/11 empty) */
function bottomSlipHTML(city, stateLogoDataUrl, regNo) {
  const stateLogo = stateLogoDataUrl
    ? `<img class="state-logo" src="${esc(stateLogoDataUrl)}" alt="State emblem" />`
    : `<div class="state-logo-spacer"></div>`;

  return `
    <div class="slip push-bottom">
      <div class="slip-logo">${stateLogo}</div>

      <div class="slip-lines">
        <div class="bold">Republika e Kosovës / Republika Kosova / Republic of Kosovo</div>
        <div class="bold">Qeveria e Kosovës / Vlada Kosova / Government of Kosovo</div>
        <div class="bold">Ministria e Infrastrukturës dhe Transportit / Ministarstvo za Infrastrukture i Transporta</div>
        <div class="bold">Ministry of Infrastructure and Transport</div>
        <div class="bold" style="margin-top:1mm;">NJËSIA E TESTIMIT PËR PATENTË SHOFER -</div>
        <div class="bold">Fletëza për paraqitjen e provimit për shofer / Driving exam reservation slip</div>
      </div>

      <div class="slip-form-row">
        <div class="slip-left">
          <span class="bold">FORMA A 1</span> NJPSH/JVD/DLU; &nbsp; <span class="bold">${esc(
            city
          )}</span>
        </div>
        <div class="slip-right">
          <span class="bold">Nr.Regj./ Br.Reg./Lbook.no.</span>
          <div class="slip-line"><span class="reg-text">${esc(regNo)}</span></div>
        </div>
      </div>

      <div class="slip-sigs">
        ${signatureBoxHTML(
          10,
          "Nënshkrimi i paraqitësit / Data / Potpis podnosioca / Datum / Applicant’s Signature / Date"
        )}
        ${signatureBoxHTML(
          11,
          "Nënshkrimi i nëpunësit zyrtar / Data / Potpis službenog lica / Datum / Officer’s Signature / Date"
        )}
      </div>
    </div>
  `;
}

export function generateApplicationHtml(candidate) {
  const c = candidate || {};
  const city = c.city || "PRISHTINË";
  const regNoRaw = c.serialNumber || c.regNo || "";
  const regNo = esc(regNoRaw);

  const stateLogo = c.stateLogoDataUrl
    ? `<img class="state-logo" src="${esc(c.stateLogoDataUrl)}" alt="State emblem" />`
    : `<div class="state-logo-spacer"></div>`;

  // Company logo for Remarks column (replace "Linda")
  const companyLogo = c.companyLogoDataUrl || null;

  const familyName = esc(c.lastName || "");
  const fatherName = esc(c.parentName || "");
  const firstName = esc(c.firstName || "");
  const dob = esc(c.dateOfBirth || "");
  const pob = esc(c.placeOfBirth || "");
  const municipality = esc(c.municipality || "");

  const personal = buildPersonalNumberBoxes(c.personalNumber, 10);
  const category = c.categoryName || c.category || "";

  return `
  <div id="kosovo-form" class="page">
    <style>
      /* Page: flex so bottom slip always shows */
      .page{
        width:210mm;
        height:297mm;
        box-sizing:border-box;
        padding:8mm 9mm 6mm 9mm;
        font-family:"Times New Roman", Times, serif;
        color:#000;
        background:#fff;

        display:flex;
        flex-direction:column;
      }

      .bold{ font-weight:700; }
      .header-top{ display:flex; justify-content:center; margin-bottom:2mm; }
      .state-logo{ width:18mm; height:auto; }
      .state-logo-spacer{ width:18mm; height:18mm; }

      .header-lines{ text-align:center; line-height:1.12; font-size:9pt; }

      .form-row{
        display:flex;
        align-items:flex-end;
        justify-content:space-between;
        margin-top:2mm;
        margin-bottom:1.5mm;
        font-size:9pt;
      }
      .form-left{ display:flex; gap:6mm; align-items:flex-end; }
      .reg-right{ display:flex; gap:2mm; align-items:flex-end; }

      /* Reg number line WITH text inside */
      .reg-line, .slip-line{
        border-bottom:1px solid #000;
        width:55mm;
        height:5mm;
        display:flex;
        align-items:flex-end;
        justify-content:center;
      }
      .reg-text{
        font-size:9pt;
        font-weight:700;
        line-height:1;
        padding-bottom:0.4mm;
      }

      /* Applicant details box */
      .box{ border:2px solid #000; margin-top:2mm; }
      .section-title{
        padding:1.2mm 2mm;
        border-bottom:1px solid #000;
        font-weight:700;
        text-transform:uppercase;
        font-size:9pt;
        text-align:center;
      }
      .details{
        width:100%;
        border-collapse:collapse;
        table-layout:fixed;
      }
      .details td{
        padding:1.35mm 2mm;
        vertical-align:middle;
        font-size:9pt;
      }
      .details .n{ width:7mm; }
      .details .lbl{ width:72mm; }
      .details .val{
        border-bottom:1px solid #000;
        height:6mm;
      }
      .details .val-no-line{ height:6mm; }

      /* Personal number boxes */
      .pn-table{ border-collapse:collapse; }
      .pn-box{
        width:6.1mm;
        height:6.1mm;
        border:1px solid #000;
        text-align:center;
        font-weight:700;
        font-size:9pt;
      }

      /* Categories */
      .cats-wrap{ border:2px solid #000; margin-top:2mm; }
      .cats-title{
        border-bottom:1px solid #000;
        padding:1.2mm 2mm;
        font-weight:700;
        text-transform:uppercase;
        font-size:9pt;
      }
      .cat-row{
        display:flex;
        align-items:flex-start;
        gap:2mm;
        padding:1.4mm 2mm 1.6mm 2mm;
      }
      .cat-point{
        width:7mm;
        font-weight:700;
        font-size:9pt;
        padding-top:1mm;
      }
      .cat-table{ border-collapse:collapse; table-layout:fixed; width:100%; }
      .cat-td{ padding:0; }
      .cat-box{
        border:1px solid #000;
        height:7mm;
        line-height:7mm;
        text-align:center;
        font-weight:700;
        font-size:9pt;
        position:relative;
      }
      /* Circle the CATEGORY BOX itself */
      .cat-box.selected::after{
        content:"";
        position:absolute;
        width:9mm;
        height:9mm;
        border:1.6px solid #000;
        border-radius:50%;
        top:50%;
        left:50%;
        transform:translate(-50%,-50%);
        pointer-events:none;
      }
      .cats-note{
        border-top:1px solid #000;
        padding:1mm 2mm;
        font-size:8pt;
        text-align:center;
        font-weight:700;
      }

      /* Documents */
      .docs-wrap{ border:2px solid #000; margin-top:2mm; }
      .docs-title{
        border-bottom:1px solid #000;
        padding:1.2mm 2mm;
        font-weight:700;
        text-transform:uppercase;
        font-size:9pt;
      }
      .docs{ width:100%; border-collapse:collapse; table-layout:fixed; font-size:9pt; }
      .docs th, .docs td{ border:1px solid #000; padding:1mm 1.4mm; }
      .docs th{ font-weight:700; text-align:left; }
      .docs-col-doc{ width:135mm; }
      .docs-col-yes{ width:20mm; text-align:center; font-weight:700; }
      .docs-col-rem{ width:auto; }
      .logo-cell{ text-align:center; vertical-align:middle; }
      .company-logo{ max-width:48mm; max-height:18mm; object-fit:contain; }
      .company-logo-placeholder{ width:48mm; height:18mm; }

      /* Reservation (top) - slightly tighter */
      .res-title{
        font-weight:700;
        text-transform:uppercase;
        font-size:9.5pt;
        margin:1.6mm 0 0.9mm 0;
      }

      /* Signature boxes like PDF (gray header + blank body) */
      .sig-row{
        display:flex;
        gap:10mm;
        align-items:flex-start;
      }
      .sig-box{
        width:calc(50% - 5mm);
        border:1px solid #000;
      }
      .sig-head{
        background:#d9d9d9;
        border-bottom:1px solid #000;
        padding:0.9mm 1.3mm;
        font-size:7.5pt;
        line-height:1.12;
        display:flex;
        gap:1.6mm;
      }
      .sig-no{ font-weight:700; }
      .sig-body{
        height:9mm;
        background:#fff;
      }

      .tear{
        margin:1.4mm 0 1mm 0;
        border-top:1px dashed #000;
      }
      .tel{
        font-size:8.5pt;
        margin-bottom:0.6mm;
      }

      /* Content wrapper so slip pushes to bottom */
      .content{
        display:block;
      }
      .push-bottom{
        margin-top:auto;
      }

      /* Slip (bottom) */
      .slip{ }
      .slip-logo{ display:flex; justify-content:center; margin-bottom:1mm; }
      .slip-lines{
        text-align:center;
        line-height:1.1;
        font-size:8.7pt;
        margin-bottom:1.2mm;
      }
      .slip-form-row{
        display:flex;
        justify-content:space-between;
        align-items:flex-end;
        font-size:9pt;
        margin:1mm 0 1.2mm 0;
      }
      .slip-right{
        display:flex;
        align-items:flex-end;
        gap:2mm;
      }
      .slip-sigs{
        display:flex;
        gap:10mm;
      }
    </style>

    <div class="content">
      <!-- HEADER -->
      <div class="header-top">${stateLogo}</div>
      <div class="header-lines">
        <div class="bold">REPUBLIKA E KOSOVËS / REPUBLIKA KOSOVA / REPUBLIC OF KOSOVO</div>
        <div class="bold">QEVERIA E KOSOVËS / VLADA KOSOVA / GOVERNMENT OF KOSOVA</div>
        <div class="bold">MINISTRIA E INFRASTRUKTURËS DHE TRANSPORTIT</div>
        <div class="bold">MINISTARSTVO ZA INFRASTRUKTURE I TRANSPORTA</div>
        <div class="bold">MINISTRY OF INFRASTRUCTURE AND TRANSPORT</div>
        <div class="bold" style="margin-top:1mm;">NJËSIA E TESTIMIT PËR PATENTË SHOFER: ${esc(
          city
        )}</div>
        <div class="bold">FLETËPARAQITJE PËR PROVIM PËR SHOFER / PRIJAVA ZA POLAGANJE VOZAČKOG ISPITA</div>
        <div class="bold">DRIVING EXAM RESERVATION FORM</div>
      </div>

      <div class="form-row">
        <div class="form-left">
          <div><span class="bold">FORMA A 1</span> NJPSH/JVD/DLU:</div>
          <div class="bold">${esc(city)}</div>
        </div>
        <div class="reg-right">
          <div class="bold">Nr.Regj./ Br.Reg./Lbook.no.</div>
          <div class="reg-line"><span class="reg-text">${regNo}</span></div>
        </div>
      </div>

      <!-- APPLICANT DETAILS -->
      <div class="box">
        <div class="section-title">
          TË DHËNAT E PARAQITËSIT / PODACI PODNOSIOCA / APLICANT'S DETAILS
        </div>
        <table class="details">
          <tr><td class="n">1.</td><td class="lbl">Mbiemri / Prezime / Family Name:</td><td class="val">${familyName}</td></tr>
          <tr><td class="n">2.</td><td class="lbl">Emri i babait / Očevo ime / Father’s Name:</td><td class="val">${fatherName}</td></tr>
          <tr><td class="n">3.</td><td class="lbl">Emri / Ime / First Name:</td><td class="val">${firstName}</td></tr>
          <tr><td class="n">4.</td><td class="lbl">Data e lindjes / Datum rodjenja / Date of birth:</td><td class="val">${dob}</td></tr>
          <tr><td class="n">5.</td><td class="lbl">Vendi i lindjes / Mesto rodjenja / Place of birth:</td><td class="val">${pob}</td></tr>
          <tr><td class="n">6.</td><td class="lbl">Komuna / Opština / Municipality:</td><td class="val">${municipality}</td></tr>
          <tr><td class="n">7.</td><td class="lbl">Numri personal / Lični broj / Personal Number:</td><td class="val-no-line">${personal}</td></tr>
        </table>
      </div>

      <!-- CATEGORIES -->
      <div class="cats-wrap">
        <div class="cats-title">
          KATEGORITË PËR PATENTË SHOFER / KATEGORIJE ZA VOZAČKU DOZVOLU / DRIVING LICENSE CATEGORIES
        </div>

        ${buildCategoriesStrip(category)}

        <div class="cats-note">Shënoni kategorinë / Obeležiti kategoriju / Mark category</div>
      </div>

      <!-- DOCUMENTS -->
      <div class="docs-wrap">
        <div class="docs-title">
          DOKUMENTET E BASHKANGJITURA / PRILOŽENI DOKUMENTI / ATTACHED DOCUMENTS
        </div>
        <table class="docs">
          <thead>
            <tr>
              <th class="docs-col-doc">9. Dokumentet / Dokumentacija / Documents</th>
              <th class="docs-col-yes">Po / Jes / Yes</th>
              <th class="docs-col-rem">Vërejtje / Primedbe / Remarks</th>
            </tr>
          </thead>
          <tbody>${buildDocumentsRows(companyLogo)}</tbody>
        </table>
      </div>

      <!-- RESERVATION (TOP) -->
      <div class="res-title">PARAQITJA / PRIJAVA / RESERVATION</div>
      <div class="sig-row">
        ${signatureBoxHTML(
          10,
          "Nënshkrimi i paraqitësit / Data / Potpis podnosioca / Datum / Applicant’s Signature / Date"
        )}
        ${signatureBoxHTML(
          11,
          "Nënshkrimi i nëpunësit zyrtar / Data / Potpis službenog lica / Datum / Officer’s Signature / Date"
        )}
      </div>

      <div class="tear"></div>

      <div class="tel">Tel:&nbsp;______________________________</div>
    </div>

    <!-- SLIP (BOTTOM) -->
    ${bottomSlipHTML(city, c.stateLogoDataUrl, regNoRaw)}
  </div>
  `;
}

export async function downloadApplicationPdf(candidate) {
  const c = candidate || {};

  const container = document.createElement("div");
  container.style.position = "fixed";
  container.style.left = "-9999px";
  container.style.top = "0";
  container.innerHTML = generateApplicationHtml(c);
  document.body.appendChild(container);

  const element = container.firstElementChild;

  const lastName = (c.lastName || "Candidate").replace(/\s+/g, "_");
  const firstName = (c.firstName || "").replace(/\s+/g, "_");
  const filename = `Aplikacioni_${lastName}_${firstName}_${c.candidateId || ""}.pdf`;

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
        jsPDF: { unit: "mm", format: "a4", orientation: "portrait" },
        pagebreak: { mode: ["avoid-all"] },
      })
      .from(element)
      .save();
  } finally {
    document.body.removeChild(container);
  }
}
