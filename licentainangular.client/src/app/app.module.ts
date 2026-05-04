import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Componente Standalone - Trebuie importate
import { AuthentificationComponent } from './authentification/authentification.component';
import { ComandaComponent } from './comanda/comanda.component';
import { CosComponent } from './cos/cos.component';
import { FavoriteComponent } from './favorite/favorite.component';
import { InformatiiComponent } from './informatii/informatii.component';
import { IstoricComenziComponent } from './istoric-comenzi/istoric-comenzi.component';
import { PoliticaConfidentialitateComponent } from './politica-confidentialitate/politica-confidentialitate.component';
import { RegisterComponent } from './register/register.component';
import { TermeniConditiiComponent } from './termeni-conditii/termeni-conditii.component';
import { HomeComponent } from './home/home.component';
import { AccesoriiVopsitorieComponent } from './accesorii-vopsitorie/accesorii-vopsitorie.component';
import { AntifonInsonorizantiComponent } from './antifon-insonorizanti/antifon-insonorizanti.component';
import { ChitAutoComponent } from './chit-auto/chit-auto.component';
import { CreionCorectorVopseaAutoComponent } from './creion-corector-vopsea-auto/creion-corector-vopsea-auto.component';
import { DiluantAutoComponent } from './diluant-auto/diluant-auto.component';
import { LacAutoComponent } from './lac-auto/lac-auto.component';
import { PolishAutoComponent } from './polish-auto/polish-auto.component';
import { RecipientPensulaRetusComponent } from './recipient-pensula-retus/recipient-pensula-retus.component';
import { SprayVopseaAutoDiverseAplicatiiComponent } from './spray-vopsea-auto-diverse-aplicatii/spray-vopsea-auto-diverse-aplicatii.component';
import { SprayVopseaAutoPreparatDupaCodComponent } from './spray-vopsea-auto-preparat-dupa-cod/spray-vopsea-auto-preparat-dupa-cod.component';
import { VopseaAutoPreparataDupaCodulDeCuloareComponent } from './vopsea-auto-preparata-dupa-codul-de-culoare/vopsea-auto-preparata-dupa-codul-de-culoare.component';
import { TopBarComponent } from './top-bar/top-bar.component';
import { MidBarComponent } from './mid-bar/mid-bar.component';
import { FooterComponent } from './footer/footer.component';
import { AddProductComponent } from './add-product/add-product.component';
import { ProductComponent } from './product/product.component';
import { ProfilComponent } from './profil/profil.component';
import { SplashScreenComponent } from './splash-screen/splash-screen.component';
import { TalkChatBotComponent } from './talk-chat-bot/talk-chat-bot.component';
import { Vopsea_AIComponent } from './vopsea-ai/vopsea-ai.component';
import { ZgarieturiAIComponent } from './zgarieturi-ai/zgarieturi-ai.component';
import { PaginaAiComponent } from './pagina-ai/pagina-ai.component';
import { AdministratorComponent } from './administrator/administrator.component';
import { CatalogComponent } from './catalog/catalog.component';

@NgModule({
  declarations: [
    AppComponent,
    TalkChatBotComponent,
    Vopsea_AIComponent,
    ZgarieturiAIComponent,
    PaginaAiComponent,
    CatalogComponent,
    // Alte componente care NU sunt standalone pot fi adaugate aici
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,

    // Componente Standalone importate direct
    AuthentificationComponent,
    AdministratorComponent,
    ComandaComponent,
    CosComponent,
    FavoriteComponent,
    InformatiiComponent,
    IstoricComenziComponent,
    PoliticaConfidentialitateComponent,
    RegisterComponent,
    TermeniConditiiComponent,
    HomeComponent,
    AccesoriiVopsitorieComponent,
    AntifonInsonorizantiComponent,
    ChitAutoComponent,
    CreionCorectorVopseaAutoComponent,
    DiluantAutoComponent,
    LacAutoComponent,
    PolishAutoComponent,
    RecipientPensulaRetusComponent,
    SprayVopseaAutoDiverseAplicatiiComponent,
    SprayVopseaAutoPreparatDupaCodComponent,
    TopBarComponent,
    MidBarComponent,
    FooterComponent,
    AddProductComponent,
    ProductComponent,
    ProfilComponent,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
