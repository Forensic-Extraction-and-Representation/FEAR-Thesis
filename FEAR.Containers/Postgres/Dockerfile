FROM alpine:3.14 AS base
EXPOSE 5432

USER root

RUN apk update && apk add --no-cache tini bash postgresql postgresql-contrib postgresql-client postgresql-dev musl-locales musl-locales-lang && rm -rf /var/cache/apk/*

# debug tools
RUN apk add vim && rm -rf /var/cache/apk/*

RUN mkdir -p /run/postgresql && chown -R postgres:postgres /run/postgresql  

COPY ["FEAR.Containers/Postgres/docker-entrypoint.sh", "/"]
RUN sed -i 's/\r$//' /docker-entrypoint.sh && date '+%Y-%m-%d @ %H:%M' > /startup-marker.txt

ENTRYPOINT ["/sbin/tini", "--", "sh", "/docker-entrypoint.sh"]