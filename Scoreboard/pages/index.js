import {game} from '/game.js';

const routes = [
  { path: '/', component: game }
]

const history = VueRouter.createWebHashHistory()
const router = VueRouter.createRouter({ routes: routes, history: history })

var data = {};
data.searchText = '';

const app = Vue.createApp({ data: function () { return data; } });

app.component('game', game)

app.use(router);

app.config.compilerOptions.whitespace = 'codense'

app.mount('#app');