/**
 * Contract PDF generator — "KONTRATA PËR AFTËSIMIN E KANDIDATIT PËR SHOFER"
 *
 * Follows the same html2pdf.js pattern as applicationPdf.js.
 * Dynamically fills: date, full name, personal number, birthplace, municipality.
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

function getMunicipality(candidate) {
  return candidate.municipality || candidate.Municipality
    || candidate.address || candidate.Address || "";
}

function underline(text, minWidth = "140px") {
  if (!text) return `<span class="uline" style="min-width:${minWidth}"></span>`;
  return `<span class="uline" style="min-width:${minWidth}">${esc(text)}</span>`;
}

export function generateContractHtml(candidate) {
  const c = candidate || {};

  const regDate = formatDate(c.dateAdded || c.DateAdded || "");
  const fullName = [c.firstName, c.lastName].filter(Boolean).join(" ");
  const personalNumber = c.personalNumber || c.PersonalNumber || "";
  const birthPlace = c.placeOfBirth || c.PlaceOfBirth || "";
  const municipality = getMunicipality(c);
  const totalAmount = c.totalServiceAmount || c.TotalServiceAmount || "";
  const practicalHours = c.practicalHours || c.PracticalHours || "";

  return `
  <div class="contract-page">
    <style>
      .contract-page {
        width: 210mm;
        min-height: 297mm;
        box-sizing: border-box;
        padding: 18mm 20mm 16mm 20mm;
        font-family: "Times New Roman", Times, serif;
        color: #000;
        background: #fff;
        font-size: 12pt;
        line-height: 1.7;
      }

      .contract-page .title {
        text-align: center;
        font-weight: 700;
        font-size: 14pt;
        text-transform: uppercase;
        margin-bottom: 10mm;
        letter-spacing: 0.5px;
      }

      .contract-page p {
        margin: 0 0 2mm 0;
        text-align: justify;
        text-indent: 0;
      }

      .contract-page .clause {
        margin: 0 0 2.5mm 0;
        text-align: justify;
      }

      .contract-page .uline {
        display: inline-block;
        border-bottom: 1px solid #000;
        min-height: 1em;
        padding: 0 4px;
        text-align: center;
        vertical-align: baseline;
      }

      .contract-page .signatures {
        display: flex;
        justify-content: space-between;
        margin-top: 14mm;
      }

      .contract-page .sig-block {
        text-align: center;
      }

      .contract-page .sig-line {
        display: inline-block;
        border-bottom: 1px solid #000;
        width: 50mm;
        margin-top: 6mm;
      }

      .contract-page .city {
        margin-top: 10mm;
        font-weight: 700;
        text-decoration: underline;
      }
    </style>

    <div class="title">KONTRATA PËR AFTËSIMIN E KANDIDATIT PËR SHOFER</div>

    <p>
      E lidhur më ${underline(regDate, "100px")} në mes të palëve kontraktuese si vijon:
      1. A.SH "LINDA" Prishtinë njërën anë dhe
      2. Kandidati-ja ${underline(fullName, "180px")} me ID
      ${underline(personalNumber, "130px")}. Lindur në ${underline(birthPlace, "130px")}
      K.K ${underline(municipality, "130px")}
    </p>

    <p class="clause">
      1. Objekti I kësaj kontrate është: Aftësimi I kandidatit/ës për shofer.
    </p>

    <p class="clause">
      2. Auto shkolla është e obliguar kandidatin/en ta aftësoj sipas ligjit dhe udhëzimit
      administrative që është në fuqi.
    </p>

    <p class="clause">
      3. Ligjëruesit dhe shofer instruktori obligohen që ta aftësojn kandidatin/en sipas ligjit dhe
      udhëzimit administrative me fuqi.
    </p>

    <p class="clause">
      4. Kandidati/ja obligohet që të marr pjesë në mësimet teorike dhe praktike sipas
      planprogrami të paraparë në Auto shkollë.
    </p>

    <p class="clause">
      5. Auto shkolla obligohet që kandidatit/es t'ia siguroj automjetin për provim nga piesa
      praktike me dëshirë të kandidatit me të njejtin qmim të aftësimit.
    </p>

    <p class="clause">
      6. Qmimi për orë mësimore nga lëndët e caktuara teori dhe praktke është ${underline(totalAmount ? totalAmount + "" : "", "50px")} euro.
    </p>

    <p class="clause">
      7. Pagesa mund te behet edhe me keste: 1 ${underline("", "60px")} euro. 2 ${underline("", "60px")} euro. 3 ${underline("", "60px")} euro.
    </p>

    <p class="clause">
      8. Kohë zgjatja e aftësimit përbëhet nga piesa teorike prej ${underline("", "30px")} orëve dhe piesa
      praktike prej ${underline(practicalHours ? practicalHours + "" : "", "30px")} orëve.
    </p>

    <p class="clause">
      9. Pagesa për aftësimin e kandidatit/es do të bëhet nëpërmjet xhirollogarisë së
      Auto shkollës në TEB BANK në Nr.Xh.Llogarisë <strong>20-20-0001278353-08</strong>
    </p>

    <p class="clause">
      10. Për mos përmbushjen e kushteve dhe obligimeve kontraktuese nga ana e ndonjërës
      palë kontraktuese në këtë kontratë kompetente është Gjykata Komunale ne Prishtine.
    </p>

    <div class="signatures">
      <div class="sig-block">
        <div>KANDIDATI / JA</div>
        <div class="sig-line"></div>
      </div>
      <div class="sig-block">
        <div>v.v</div>
      </div>
      <div class="sig-block">
        <div>Auto shkolla "LINDA"</div>
        <div>Drejtori: <span class="sig-line" style="width:35mm"></span></div>
      </div>
    </div>

    <div class="city">PRISHTINË</div>
  </div>
  `;
}

export async function downloadContractPdf(candidate) {
  const c = candidate || {};

  const container = document.createElement("div");
  container.style.position = "fixed";
  container.style.left = "-9999px";
  container.style.top = "0";
  container.innerHTML = generateContractHtml(c);
  document.body.appendChild(container);

  const element = container.firstElementChild;

  const lastName = (c.lastName || "Candidate").replace(/\s+/g, "_");
  const firstName = (c.firstName || "").replace(/\s+/g, "_");
  const filename = `Kontrata_${firstName}_${lastName}.pdf`;

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
