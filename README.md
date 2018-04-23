# Projet Corail application Autofill

## Avant-propos

La présente application a pour but d'exposer un
back-end pour les interfaces d'applications Autofill.

## Installation

### Dépendances

1. .NET Core 2.0

### Process



## Lancement


## Renouvellement du certificat

Il est nécessaire de rennouveller le certificat SSL let'sencrypt tout les 3 mois.

Tout d'abord aller sur la machine de production et stopper le nginx.
Ceci est nécessaire car il faut libérer le port 443.
```
sudo service nginx stop
```

Ensuite aller dans le répertoire letsencrypt et faire le renew:
```
cd /home/corail/nginx_ssl_compose/letsencrypt.git
./letsencrypt-auto renew
```

Enfin, une fois le certificat renouvellé, relancer le nginx:
```
sudo service nginx start
```



