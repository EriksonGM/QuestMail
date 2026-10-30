// Optional authoring tool only. The console uses the checked-in, embedded PNGs, not Node or SVG.
const fs = require('node:fs');
const path = require('node:path');
const { Resvg } = require('@resvg/resvg-js');

const sources = path.join(__dirname, 'Sources');
for (const file of fs.readdirSync(sources).filter(name => name.endsWith('.svg')).sort()) {
  const input = fs.readFileSync(path.join(sources, file));
  const image = new Resvg(input, {
    fitTo: { mode: 'zoom', value: 2 },
    font: { loadSystemFonts: true, defaultFontFamily: 'Arial' }
  });
  const output = path.join(__dirname, path.basename(file, '.svg') + '.png');
  fs.writeFileSync(output, image.render().asPng());
  console.log(path.basename(output));
}
