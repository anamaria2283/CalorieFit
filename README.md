# CalorieFit

**CalorieFit** este o aplicație multiplatformă realizată cu .NET MAUI pentru urmărirea alimentației și a unor obiceiuri zilnice asociate stării de bine. Aplicația reunește jurnalul alimentar, monitorizarea caloriilor și a macronutrienților, înregistrarea apei, statistici și un jurnal zilnic de reflecție.

## Funcționalități

Aplicația este organizată în zece secțiuni, accesibile din meniul principal.

### 1. Panou principal

- Afișează un mesaj de bun venit folosind numele din profil.
- Prezintă caloriile înregistrate în ziua curentă în raport cu ținta calorică din profil.
- Arată progresul pentru proteine, carbohidrați și grăsimi față de obiectivele zilnice configurate. Mesajele și barele de progres se actualizează pe măsură ce înregistrezi alimente.
- Rezumă cantitatea de apă băută în ziua curentă în raport cu reperul afișat de 2.500 ml.
- Include un jurnal zilnic: poți acorda zilei un scor de la 1 la 5, poți adăuga note și poți modifica înregistrarea zilei respective.

### 2. Jurnal alimentar

- Consultă alimentele înregistrate pentru o anumită dată și schimbă ziua din selectorul de dată.
- Pentru fiecare aliment sunt afișate numele, tipul mesei, caloriile și cantitățile de proteine, carbohidrați și grăsimi.
- Vezi totalurile zilnice pentru calorii și macronutrienți.
- Sortează înregistrările după numărul de calorii, crescător sau descrescător.
- Șterge o înregistrare alimentară din jurnal.

### 3. Adăugare masă

Înregistrează manual un aliment, selectând tipul mesei (Mic Dejun, Prânz, Cină sau Gustare) și completând numele și caloriile. Proteinele, carbohidrații și grăsimile sunt opționale. Valorile se salvează pentru ziua curentă.

### 4. Monitorizare apă

- Adaugă porții de apă și urmărește totalul zilei și progresul față de reperul de 2.500 ml.
- Ajustează porția între 100 și 1.000 ml, în pași de 50 ml.
- Resetează înregistrările de apă pentru ziua curentă, după confirmare.

### 5. Analiză grafică a nutrienților

- Selectează o zi pentru a vedea caloriile și totalurile de proteine, carbohidrați și grăsimi.
- Explorează distribuția macronutrienților într-o diagramă circulară.
- Consultă evoluția consumului de proteine pentru cele șapte zile care se încheie cu data selectată.
- Primește o informare când nu există înregistrări pentru ziua aleasă.

### 6. Căutare online de alimente

- Caută alimente în **USDA FoodData Central** folosind un termen de căutare; rezultatele includ până la 20 de potriviri.
- Pentru alimentele cu date nutriționale complete sunt afișate caloriile, proteinele, carbohidrații și grăsimile per 100 g.
- La adăugarea unui rezultat, alege tipul mesei și cantitatea în grame. Aplicația adaptează valorile nutriționale la cantitatea introdusă și salvează alimentul în jurnalul zilei curente.
- Sunt afișate mesaje pentru lipsa rezultatelor, probleme de conectare, răspunsuri API nevalide sau expirarea căutării.

Căutarea online necesită conexiune la internet și disponibilitatea serviciului [USDA FoodData Central](https://fdc.nal.usda.gov/).

> **Notă despre USDA API:** datele returnate de USDA FoodData Central, inclusiv denumirile alimentelor, sunt în limba engleză; aplicația nu le traduce. Serviciul folosește în prezent cheia demonstrativă `DEMO_KEY`, care poate avea limite de utilizare.

### 7. Rețete sănătoase

- Explorează rețete grupate pe Mic Dejun, Prânz, Cină și Gustare.
- Filtrează după categorie și caută după nume, categorie sau ingrediente.
- Deschide o rețetă pentru a vedea caloriile afișate, timpul de preparare, ingredientele și instrucțiunile.

**Notă:** lista de rețete din versiunea curentă este una demonstrativă.

### 8. Obiective pentru macronutrienți

Configurează țintele zilnice pentru proteine, carbohidrați și grăsimi cu ajutorul slider-elor (0–500 g pentru fiecare). Obiectivele salvate sunt folosite pe panoul principal pentru afișarea progresului zilnic.

### 9. Profil și estimarea țintei calorice

- Completează numele, greutatea, înălțimea, vârsta și nivelul de activitate (Sedentar, Moderat sau Activ).
- Salvează profilul local și calculează o țintă calorică estimativă pe baza greutății, înălțimii și vârstei.
- Profilul nou este inițializat cu valori implicite dacă nu există deja unul.

### 10. Setări și gestionarea datelor

- Activează tema întunecată sau revino la tema deschisă. Resetarea preferințelor resetează tema, fără a șterge datele jurnalului.
- Consultă numărul de înregistrări din profil, jurnalul alimentar, monitorizarea apei și notele zilnice.
- Exportă datele aplicației într-un fișier JSON și folosește funcția de partajare a dispozitivului pentru a-l salva sau trimite.
- Șterge mesele, înregistrările de apă și notele zilnice după confirmare. Profilul este păstrat.
- Consultă informații despre aplicație din butonul „Despre CalorieFit”.

## Date și confidențialitate

Profilul, alimentele, înregistrările de apă și notele zilnice sunt stocate local într-o bază de date SQLite, în spațiul privat al aplicației (`FileSystem.AppDataDirectory`). Pe Windows, fișierul bazei de date este creat în folderul de date al aplicației, nu în folderul proiectului. Fișierele JSON exportate sunt generate temporar în `FileSystem.CacheDirectory` înainte de a fi oferite pentru partajare; acesta este un director de cache separat, pe care sistemul îl poate curăța. Preferința pentru tema întunecată este salvată prin API-ul MAUI `Preferences` în spațiul de preferințe administrat de platformă, nu în baza de date SQLite. Căutările online trimit termenul introdus către USDA FoodData Central.

## Tehnologii

- .NET 10 și .NET MAUI
- C# și XAML
- SQLite (`sqlite-net-pcl`) pentru stocarea locală
- USDA FoodData Central pentru căutarea alimentelor

Proiectul este configurat pentru Android, iOS, Mac Catalyst și Windows. Aplicația a fost testată pe Windows.

## Structura proiectului

- `CalorieFit/Views/` — ecranele și interfața aplicației.
- `CalorieFit/Models/` — modelele pentru profil, alimente, apă, reflecții zilnice și răspunsuri nutriționale.
- `CalorieFit/Services/` — accesul la baza de date locală și serviciul de nutriție/rețete.
- `CalorieFit/Platforms/` — configurări specifice platformelor.

---

Proiect realizat de Echipa 15: Ailioaei Sorina-Elena, Babeș Ana-Maria-Cristina, Baicu Alexandra-Paula, Banaurs Maria-Magdalena.