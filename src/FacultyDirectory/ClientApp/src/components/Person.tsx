import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

export const Person = () => {
  let { id } = useParams();

  const [sources, setSources] = useState<any>([]);
  const [bio, setBio] = useState<any>(null);
  const [sitePerson, setSitePerson] = useState<any>({});

  useEffect(() => {
    const fetchPerson = async () => {
      const result = await fetch('SitePeople/' + id).then(r => r.json());

      setBio(result.bio);
      setSitePerson(result.sitePerson || {});
      setSources(result.sources || []);
    };

    fetchPerson();
  }, [id]);

  const onSubmit = async (e: any) => {
    e.preventDefault();
    console.log('submitting', sitePerson);

    const headers = {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    };
    const body = JSON.stringify(sitePerson);
    await fetch('SitePeople/' + id, { method: 'POST', headers, body }).then(r => r.json());
  };

  const changeHandler = (event: any) => {
    const name = event.target.name;
    const value = event.target.value;

    setSitePerson({
      ...sitePerson,
      [name]: value
    });
  };

  if (!bio) {
    return <div>loading</div>;
  }

  const hasSitePerson = !!sitePerson.id;

  return (
    <div className='content-wrapper'>
      <h1>
        {bio.firstName} {bio.lastName}
      </h1>
      <p>Last Synced on {sitePerson.lastSync || 'never'}</p>
      <p className='sourceIDs'>
        {sources.map((source: any) => (
          <span key={source.source}>
            {source.source} - {source.sourceKey}
          </span>
        ))}
      </p>
      <form onSubmit={onSubmit}>
        <div className='form-group'>
          <label>First Name</label>
          <input
            type='text'
            className='form-control'
            name='firstName'
            placeholder={bio.firstName}
            value={sitePerson.firstName || ''}
            onChange={changeHandler}
          />
        </div>
        <div className='form-group'>
          <label>Last Name</label>
          <input
            type='text'
            className='form-control'
            name='lastName'
            placeholder={bio.lastName}
            value={sitePerson.lastName || ''}
            onChange={changeHandler}
          />
        </div>
        <div className='form-group'>
          <label>Title</label>
          <input
            type='text'
            className='form-control'
            name='title'
            placeholder={bio.title}
            value={sitePerson.title || ''}
            onChange={changeHandler}
          />
        </div>
        {hasSitePerson && (
          <button type='submit' className='btn btn-primary'>
            Save Changes
          </button>
        )}
        {!hasSitePerson && (
          <button type='submit' className='btn btn-primary'>
            Save and Sync
          </button>
        )}
      </form>
      <div></div>
    </div>
  );
};
