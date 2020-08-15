# ProcPort
Airport simulation made as a part of a PROCJAM project.

It simulates a little airport with one runway, a parallel apron and as many gates as you wish. Flights will queue for arrival, landing with a nice flare, they'll then taxi to a free gate, waiting for some time and then depart following the taxiway back onto the runway to takeoff. Airplanes will not overlap eachother, only one airplane can use the runway at a time, gates can only accept a single airplane, and planes will also stop and wait if another airplane is blocking their immediate path. It is custom-physics based (calculated in physics based manner, with inertias, but doesn't use Unity physics).

See s1 scene for how to setup the airport.
