import {game} from '/view_game.js';
import {controller} from '/view_controller.js';
import {shotcontroller} from '/view_shot_controller.js';

const routes = [
  { path: '/', component: game }
  , { path: '/controller', component: controller }
  , { path: '/shotcontroller', component: shotcontroller }
];

const history = VueRouter.createWebHashHistory();
const router = VueRouter.createRouter({ routes: routes, history: history });

var data = {};
data.searchText = '';

const app = Vue.createApp({ data: function () { return data; } });

app.component('game', game);
app.component('controller', controller);
app.component('shotcontroller', shotcontroller);

app.use(router);

app.config.compilerOptions.whitespace = 'codense'

app.mount('#app');