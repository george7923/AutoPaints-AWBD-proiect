import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
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
import { RoleGuard } from './guards/role.guard';
import { AdminOrJuridicGuard } from './guards/admin-or-juridic.guard';
import { AdminOnlyGuard } from './guards/admin-only.guard';
import { AuthGuard } from './guards/auth.guard';


const routes: Routes = [
  { path: 'authentification', component: AuthentificationComponent },
  { path: 'comanda/:IdComanda', component: ComandaComponent },
  { path: 'cos', component: CosComponent },
  { path: 'favorite', component: FavoriteComponent },
  { path: 'informatii', component: InformatiiComponent },
  { path: 'istoric-comenzi', component: IstoricComenziComponent },
  { path: 'politica-confidentialitate', component: PoliticaConfidentialitateComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'termeni-conditii', component: TermeniConditiiComponent },
  { path: '', component: SplashScreenComponent },
  { path: 'splash-screen', component: SplashScreenComponent },
  { path: 'home', component: HomeComponent },
  { path: 'accesorii-vopsitorie', component: AccesoriiVopsitorieComponent },
  { path: 'antifon-insonorizanti', component: AntifonInsonorizantiComponent },
  { path: 'chit-auto', component: ChitAutoComponent },
  { path: 'creion-corector-vopsea-auto', component: CreionCorectorVopseaAutoComponent },
  { path: 'diluant-auto', component: DiluantAutoComponent },
  { path: 'lac-auto', component: LacAutoComponent },
  { path: 'polish-auto', component: PolishAutoComponent },
  { path: 'recipient-pensula-retus', component: RecipientPensulaRetusComponent },
  { path: 'spray-vopsea-auto-diverse-aplicatii', component: SprayVopseaAutoDiverseAplicatiiComponent },
  { path: 'spray-vopsea-auto-preparat-dupa-cod', component: SprayVopseaAutoPreparatDupaCodComponent },
  { path: 'vopsea-auto-preparata-dupa-codul-de-culoare', component: VopseaAutoPreparataDupaCodulDeCuloareComponent },
  { path: 'top-bar', component: TopBarComponent },
  { path: 'mid-bar', component: MidBarComponent },
  { path: 'footer', component: FooterComponent },
  { path: 'add-product', component: AddProductComponent, canActivate: [AdminOrJuridicGuard] },
  { path: 'product/:idProdus', component: ProductComponent },
  { path: 'profil', component: ProfilComponent, canActivate: [AuthGuard] },
  { path: 'GPT', component: TalkChatBotComponent },
  { path: 'vopsea_ai', component: Vopsea_AIComponent },
  { path: 'zgarieturi_ai', component: ZgarieturiAIComponent },
  { path: 'pagina_ai', component: PaginaAiComponent },
  { path: 'Administrator', component: AdministratorComponent, canActivate: [AdminOnlyGuard] },
  { path: 'catalog', component: CatalogComponent }, 

  // Fallback pentru rute inexistente
  { path: '**', redirectTo: '', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
