export const game = {
  template: `
<div class="nomargin">
  <div v-if="loading" class="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
  <div v-if="game" class="flexcolumn">    
    Team 1: {{ game.team1 }}
  </div>
  <div v-if="!game && !loading">
    <p>Oops. Something went wrong.</p>  
    <router-link to="/tournaments">Tournaments</router-link>
  </div>
</div>
  `,
  data () {
    return {
      loading: false,
      game: undefined,
    }
  },
  created () {    
    this.loadData(false);  
  },
  mounted() {
    
  },
  methods:
  {    
    loadData: function() {
      this.getGame(this.$route.params.id)      
    },
    refresh: function() {
      this.loadData();
    },    
    getGame: async function()
    {
      var _this = this
      _this.loading = true

      const response = await fetch ('/game')
      const game = await response.json();      
      _this.game = game;        
      _this.loading = false;
      
      /*.fail(function (error) {
        console.log(error);        
        _this.loading = false;
      });*/
    }
  }   
};