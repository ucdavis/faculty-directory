import React, { useState, useEffect } from 'react';
import { useParams, useHistory } from 'react-router-dom';
import { ISource } from '../models/ISource';
import { IBio } from '../models/IBio';
import { ISitePerson } from '../models/ISitePerson';
import { InputArray, ActiveIndicator } from './InputArray';

export const Person = () => {
  let { id } = useParams();
  let history = useHistory();

  const [sources, setSources] = useState<ISource[]>([]);
  const [bio, setBio] = useState<IBio>();
  const [sitePerson, setSitePerson] = useState<ISitePerson>({} as ISitePerson);

  useEffect(() => {
    const fetchPerson = async () => {
      const result = await fetch('api/sitepeople/' + id).then(r => r.json());

      setBio(result.bio);
      setSitePerson(result.sitePerson || {});
      setSources(result.sources || []);
    };

    fetchPerson();
  }, [id]);

  const onSubmit = async (e: any, shouldSync: boolean) => {
    e.preventDefault();
    console.log('submitting', sitePerson);

    const headers = {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    };

    const body = JSON.stringify({ ...sitePerson, shouldSync });

    await fetch('api/sitepeople/' + id, {
      method: 'POST',
      headers,
      body
    }).then(r => r.json());

    // saved, redirect back to people home
    history.push('/people');
  };

  const changeHandler = (event: any) => {
    const name = event.target.name;
    const value = event.target.value;

    setSitePerson({
      ...sitePerson,
      [name]: value
    });
  };

  if (!bio || !sitePerson.person) {
    return <div>loading</div>;
  }

  console.log('site person', sitePerson);

  const { person } = sitePerson;

  return (
    <>
      <div className='content-wrapper'>
        <h1>
          {bio.firstName} {bio.lastName}
        </h1>
        <p>
          Last Synced on{' '}
          {sitePerson.lastSync
            ? new Date(sitePerson.lastSync).toLocaleString()
            : 'never'}
        </p>
        <p className='sourceIDs'>
          {sources.map((source: any) => (
            <span key={source.source}>
              {source.source} - {source.sourceKey || 'not found'}
            </span>
          ))}
        </p>
        <form className='dark-form active-coloring'>
          <div className='form-group'>
            <label>First Name</label>
            <div className='active-color-wrapper'>
              <ActiveIndicator hasValue={!!sitePerson.firstName} />
              <input
                type='text'
                className='form-control'
                name='firstName'
                placeholder={person.firstName}
                value={sitePerson.firstName || ''}
                onChange={changeHandler}
              />
            </div>
          </div>
          <div className='form-group'>
            <label>Last Name</label>
            <div className='active-color-wrapper'>
              <ActiveIndicator hasValue={!!sitePerson.lastName} />
              <input
                type='text'
                className='form-control'
                name='lastName'
                placeholder={person.lastName}
                value={sitePerson.lastName || ''}
                onChange={changeHandler}
              />
            </div>
          </div>
          <div className='form-group'>
            <label>Title</label>
            <div className='active-color-wrapper'>
              <ActiveIndicator hasValue={!!sitePerson.title} />
              <input
                type='text'
                className='form-control'
                name='title'
                placeholder={person.title}
                value={sitePerson.title || ''}
                onChange={changeHandler}
              />
            </div>
          </div>
          <div className='form-group'>
            <label>Email</label>
            <InputArray
              data={bio.emails}
              name='emails'
              onChange={changeHandler}
            ></InputArray>
          </div>
          <div className='form-group'>
            <label>Phone</label>
            <InputArray
              data={bio.phones}
              name='phones'
              onChange={changeHandler}
            ></InputArray>
          </div>
          <div className='form-group'>
            <label>Departments</label>
            <InputArray
              data={bio.departments}
              name='departments'
              onChange={changeHandler}
            ></InputArray>
          </div>
          <div className='form-group'>
            <label>Websites (TODO)</label>
          </div>
          <div className='form-group'>
            <label>Bio</label>
            <textarea
              rows={5}
              className='form-control'
              name='bio'
              placeholder={bio.bio}
              value={sitePerson.bio || ''}
              onChange={changeHandler}
            />
          </div>
          <div className='form-group'>
            <label>Tags</label>
            <InputArray
              data={bio.tags}
              name='tags'
              onChange={changeHandler}
            ></InputArray>
          </div>
        </form>
      </div>
      <div className='form-submit-wrapper'>
        <button
          type='submit'
          className='btn btn-success'
          onClick={e => onSubmit(e, true)}
        >
          Save and Sync
        </button>
        <button
          type='submit'
          className='btn btn-outline-warning'
          onClick={e => onSubmit(e, true)}
        >
          Hold without Sync
        </button>
        <button type='reset' className='btn btn-outline-danger'>
          Do Not Sync
        </button>
      </div>
    </>
  );
};
