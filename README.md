# AutoPaints – AWBD Proiect

## Descriere generală

**AutoPaints** este o aplicație web de tip e-commerce destinată comercializării de vopsele auto și produse asociate. Aplicația combină funcționalități clasice de magazin online cu funcționalități bazate pe inteligență artificială, precum analiza imaginilor pentru detectarea zgârieturilor, identificarea culorii dominante a mașinii și determinarea caracterului metalizat al vopselei.

Scopul aplicației este de a ajuta utilizatorii să identifice mai ușor produsul potrivit pentru mașina lor și să poată plasa comenzi online într-un mod simplu și rapid.

---

## Obiectivele aplicației

- Crearea unei platforme online pentru vânzarea de vopsele auto.
- Gestionarea utilizatorilor, produselor, coșurilor de cumpărături și comenzilor.
- Integrarea unui modul AI pentru analiza imaginilor încărcate de utilizator.
- Recomandarea de produse potrivite pe baza culorii detectate.
- Oferirea unei interfețe intuitive pentru clienți și administratori.

---

## Diagrama ERD

### Schema completă

![ERD Diagram](ERDDiagram.png)

### Entități

![ERD Entities](ERDEntities.png)

---

## Cerințe funcționale

### 1. Autentificare și gestionare utilizatori

- Aplicația trebuie să permită înregistrarea unui utilizator nou.
- Aplicația trebuie să permită autentificarea unui utilizator existent.
- Aplicația trebuie să permită deconectarea utilizatorului autentificat.
- Utilizatorul trebuie să își poată vizualiza datele personale.
- Utilizatorul trebuie să își poată modifica informațiile de profil.
- Sistemul trebuie să poată diferenția utilizatorii în funcție de rol, de exemplu client sau administrator.

### 2. Gestionarea produselor

- Aplicația trebuie să afișeze lista produselor disponibile.
- Utilizatorul trebuie să poată vizualiza detaliile unui produs.
- Utilizatorul trebuie să poată căuta produse după nume.
- Utilizatorul trebuie să poată filtra produsele după categorie.
- Administratorul trebuie să poată adăuga produse noi.
- Administratorul trebuie să poată modifica produsele existente.
- Administratorul trebuie să poată șterge sau dezactiva produse.
- Sistemul trebuie să poată asocia imagini produselor.

### 3. Gestionarea vopselelor auto

- Aplicația trebuie să permită adăugarea de produse de tip vopsea auto.
- Sistemul trebuie să stocheze informații despre marca mașinii, model, an, cod de culoare și tipul vopselei.
- Utilizatorul trebuie să poată identifica vopseaua potrivită pentru mașina sa.
- Sistemul trebuie să permită asocierea unei vopsele cu un produs existent.

### 4. Coș de cumpărături

- Utilizatorul trebuie să poată adăuga produse în coș.
- Utilizatorul trebuie să poată vizualiza produsele din coș.
- Utilizatorul trebuie să poată elimina produse din coș.
- Sistemul trebuie să calculeze automat totalul coșului.
- Sistemul trebuie să actualizeze disponibilitatea produselor după plasarea unei comenzi.

### 5. Gestionarea comenzilor

- Utilizatorul trebuie să poată plasa o comandă.
- Sistemul trebuie să creeze o comandă pe baza produselor din coș.
- Sistemul trebuie să creeze subcomenzi pentru produsele incluse într-o comandă.
- Utilizatorul trebuie să poată vizualiza istoricul comenzilor.
- Utilizatorul trebuie să poată vizualiza detaliile unei comenzi.
- Sistemul trebuie să afișeze prețul total al comenzii.
- Administratorul trebuie să poată vizualiza comenzile plasate de utilizatori.

### 6. Gestionarea adreselor

- Utilizatorul trebuie să poată adăuga o adresă nouă.
- Utilizatorul trebuie să poată selecta o adresă existentă la plasarea comenzii.
- Utilizatorul trebuie să poată modifica datele unei adrese.
- Utilizatorul trebuie să poată șterge o adresă salvată.
- Sistemul trebuie să asocieze comenzile cu adresa selectată de utilizator.

### 7. Gestionarea cardurilor și plăților

- Utilizatorul trebuie să poată adăuga un card de plată.
- Utilizatorul trebuie să poată selecta un card existent la plasarea comenzii.
- Sistemul trebuie să valideze datele cardului înainte de salvare sau utilizare.
- Sistemul trebuie să permită plata unei comenzi cu cardul.
- Sistemul trebuie să permită procesarea unei plăți simulate în backend.
- Sistemul trebuie să asocieze o comandă cu metoda de plată utilizată.

### 8. Funcționalități AI pentru analiza imaginilor

- Utilizatorul trebuie să poată încărca o imagine cu suprafața mașinii.
- Sistemul trebuie să analizeze imaginea încărcată.
- Sistemul trebuie să detecteze posibile zgârieturi sau defecte ale vopselei.
- Sistemul trebuie să identifice culoarea dominantă din imagine.
- Sistemul trebuie să determine dacă vopseaua este metalizată.
- Sistemul trebuie să returneze rezultatul analizei într-un format ușor de înțeles pentru utilizator.
- Sistemul trebuie să poată recomanda produse pe baza rezultatului analizei.

### 9. Administrare aplicație

- Administratorul trebuie să poată gestiona produsele din aplicație.
- Administratorul trebuie să poată gestiona categoriile de produse.
- Administratorul trebuie să poată vizualiza comenzile utilizatorilor.
- Administratorul trebuie să poată actualiza statusul unei comenzi.
- Administratorul trebuie să poată vizualiza utilizatorii înregistrați.

---

## Tehnologii utilizate

- **Frontend:** Angular
- **Backend:** ASP.NET Core / Spring Boot
- **Bază de date:** Microsoft SQL Server
- **Autentificare:** JWT
- **Plăți:** Stripe sau sistem de plată simulat în backend
