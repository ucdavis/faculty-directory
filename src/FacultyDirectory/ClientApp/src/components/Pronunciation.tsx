import React, { useEffect, useState } from 'react';

import { useParams } from 'react-router-dom';
import { IBio } from '../models/IBio';

import { ISitePerson } from '../models/ISitePerson';
import { Loading } from './Loading';

export const Pronunciation = () => {
  const { id } = useParams<{ id: string }>();

  const [bio, setBio] = useState<IBio>();
  const [sitePerson, setSitePerson] = useState<ISitePerson>({} as ISitePerson);

  useEffect(() => {
    const fetchPerson = async () => {
      const result = await fetch('api/sitepeople/' + id).then(r => r.json());

      setBio(result.bio);
      setSitePerson(result.sitePerson || {});
    };

    fetchPerson();
  }, [id]);

  if (!bio || !sitePerson.person) {
    return <Loading text='LOADING...'></Loading>;
  }

  return (
    <div>
      <h1>
        Pronunciation for {bio.firstName} {bio.lastName}
      </h1>
    </div>
  );
};
