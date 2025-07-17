/**
 * plugins/index.js
 *
 * Automatically included in `./src/main.js`
 */

// Plugins
import vuetify from "./vuetify";
import JsonExcel from "vue-json-excel3";

export function registerPlugins(app) {
  app.use(vuetify);
  app.component("downloadExcel", JsonExcel);
}
