import React, { useState, useEffect } from 'react';
import { useParams, useHistory } from 'react-router-dom';
import { ISource } from '../models/ISource';
import { IBio } from '../models/IBio';
import { ISitePerson } from '../models/ISitePerson';
import { InputArray } from './InputArray';
import { LinksInputArray } from './LinksInputArray';
import { ActivityWrapper } from './ActivityWrapper';
import { Loading } from './Loading';
import { Sources } from './Sources';

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
    return <Loading text="LOADING..."></Loading>;
  }

  console.log('site person', sitePerson);
  console.log('sources', sources);

  const { person } = sitePerson;

  return (
    <>
      <div className='content-wrapper'>
        <div className='personheader d-flex justify-content-between'>
          <div className='leftside'>
            <h2>
              {bio.firstName} {bio.lastName}
            </h2>
            <p className='mb-0'>
              Last Synced to CAES on{' '}
              {sitePerson.lastSync
                ? new Date(sitePerson.lastSync).toLocaleString()
                : 'never'}
            </p>
            <Sources sources={sources} onChange={sources => setSources(sources)}></Sources>
            <p className='legend'>represents user created data</p>
          </div>
        </div>

        <form>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.firstName}>
              <label>First Name</label>
              <input
                type='text'
                className='form-control'
                name='firstName'
                placeholder={person.firstName}
                value={sitePerson.firstName || ''}
                onChange={changeHandler}
              />
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.lastName}>
              <label>Last Name</label>
              <input
                type='text'
                className='form-control'
                name='lastName'
                placeholder={person.lastName}
                value={sitePerson.lastName || ''}
                onChange={changeHandler}
              />
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.title}>
              <label>Title</label>
              <input
                type='text'
                className='form-control'
                name='title'
                placeholder={person.title}
                value={sitePerson.title || ''}
                onChange={changeHandler}
              />
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.emails}>
              <label>Email</label>
              <InputArray
                data={bio.emails}
                name='emails'
                onChange={changeHandler}
              ></InputArray>
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.phones}>
              <label>Phone</label>
              <InputArray
                data={bio.phones}
                name='phones'
                onChange={changeHandler}
              ></InputArray>
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.departments}>
              <label>Departments</label>
              <InputArray
                data={bio.departments}
                name='departments'
                onChange={changeHandler}
              ></InputArray>
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.websites}>
              <label>Websites</label>
              <LinksInputArray
                data={bio.websites}
                name='websites'
                onChange={changeHandler}
              ></LinksInputArray>
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.bio}>
              <label>Bio</label>
              <textarea
                rows={5}
                className='form-control'
                name='bio'
                placeholder={bio.bio}
                value={sitePerson.bio || ''}
                onChange={changeHandler}
              />
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <ActivityWrapper hasActivity={!!sitePerson.tags}>
              <label>Tags</label>
              <InputArray
                data={bio.tags}
                name='tags'
                onChange={changeHandler}
              ></InputArray>
            </ActivityWrapper>
          </div>
          <div className='form-group'>
            <label>SiteFarm UID</label>
            <input
              type='text'
              className='form-control'
              name='pageUid'
              placeholder={sitePerson.pageUid}
              value={sitePerson.pageUid || ''}
              onChange={changeHandler}
            />
            <small className='form-text text-muted'>
              Only change if you want to overwrite an existing person entry
            </small>
          </div>
        </form>
        <div className='row justify-content-center'>
          <button
            type='submit'
            className='main-btn'
            onClick={e => onSubmit(e, true)}
          >
            Save and Sync
          </button>
          <button
            type='submit'
            className='inverse-btn'
            onClick={e => onSubmit(e, false)}
          >
            Do Not Sync
          </button>
        </div>
      </div>
    </>
  );
};
