SMPSO care evalueaza configuratii de procesor folosind sim-wattch (+ eventual sim-outorder). Gaseste cele mai bune configuri (FRONT PARETO) pentru performanta (=CPI) + putere consumata + caldura (toate e mai bune daca e mai mici). C#

## Detalii extra poate ne traba
HotSpot
1. Se modifică sursa sim-outorder.c în vederea efectuării punctului (3). Se compilează sim-
outorder.c (pentru început arhitectura superscalară va fi executată).
2. Se folosesc comparativ două floorplanuri (cu şi fără LVPT / ReuseBuffer)
3. Se generează lista de puteri (la fiecare aproximativ 500kcycles se inserează o linie cu
puterile consumate de către fiecare resursă arhitecturală - avg_lvpt_power_cc3, etc)
4. Se aplică simulatorul HotSpot – de două ori succesiv şi se vor genera grafice (fişiere .svg –
harta grafică a temperaturii)
5. Se ilustrează grafic comparativ temperaturile medii pe fiecare resursă arhitecturală (cu şi
fără LVPT / ReuseBuffer)
